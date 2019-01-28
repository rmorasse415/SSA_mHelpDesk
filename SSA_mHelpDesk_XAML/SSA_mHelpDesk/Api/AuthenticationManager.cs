using IdentityModel.OidcClient;
using SSA_mHelpDesk.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSA_mHelpDesk.API
{
    public sealed class AuthenticationManager
    {
        private static readonly string authFileLocation = ".auth.dat";

        private static readonly Lazy<AuthenticationManager> lazy =
            new Lazy<AuthenticationManager>(() => new AuthenticationManager());

        public static AuthenticationManager Instance { get { return lazy.Value; } }

        private readonly Mutex mRetrieveThreadMutex = new Mutex();
        private readonly Object mAuthLock = new Object();

        public class AuthenticationInfo
        {
            public string AccessToken;
            public string RefreshToken;
            public DateTime AccessTokenExpiration;
            public string ApiKey;

            public bool IsExpired()
            {
                return AccessTokenExpiration < DateTime.Now;
            }
        }

        private AuthenticationInfo mAuthInfo;
        Task<AuthenticationInfo> mRetrieveTask;

        private AuthenticationManager()
        {
            mRetrieveTask = ReadAuthAsync();
        }

        //public AuthenticationInfo GetAuthInfo()
        public async Task<AuthenticationInfo> GetAuthInfoAsync()
        {
            //lock (mRetrieveTaskLock)
            mRetrieveThreadMutex.WaitOne();
            {
                if (mRetrieveTask != null)
                {
                    AuthenticationInfo newAuthInfo = await mRetrieveTask;
                    mRetrieveTask = null;

                    //don't save new auth if it isn't valid but the old one is
                    lock (mAuthLock)
                    {
                        if (!IsAuthValid(mAuthInfo) || IsAuthValid(newAuthInfo))
                        {
                            mAuthInfo = newAuthInfo;
                        }
                    }
                }
            }
            mRetrieveThreadMutex.ReleaseMutex();

            //If our auth is expired and renewable then block until we get one that is ready
            if (!IsAuthValid(mAuthInfo) && IsAuthRenewable(mAuthInfo))
            {
                mAuthInfo = await RenewAuthAsync(mAuthInfo.RefreshToken);
            }
            else
            {
                //Launch another renew in the background if we are close to expiration
                mRetrieveThreadMutex.WaitOne();
                if (mRetrieveTask == null && 
                    IsAuthRenewable(mAuthInfo) && 
                    mAuthInfo.AccessTokenExpiration < DateTime.Now.AddHours(1))
                {
                    mRetrieveTask = RenewAuthAsync(mAuthInfo.RefreshToken);
                }
                mRetrieveThreadMutex.ReleaseMutex();
            }

            return mAuthInfo;
        }

        private async Task<AuthenticationInfo> ReadAuthAsync()
        {
            AuthenticationInfo authInfo = await ReadAuthFromFile();

            if (authInfo != null && authInfo.IsExpired() && authInfo.RefreshToken.Length > 0)
            {
                authInfo = await RenewAuthAsync(authInfo.RefreshToken);
            }

            return authInfo;
        }
        
        public bool RenewAuth(String refreshToken)
        {
            if (refreshToken == null)
                return false;

            mRetrieveThreadMutex.WaitOne();
            {
                mRetrieveTask = RenewAuthAsync(refreshToken);
            }
            mRetrieveThreadMutex.ReleaseMutex();

            return true;
        }

        private async Task<AuthenticationInfo> RenewAuthAsync(String refreshToken)
        {
            var options = new OidcClientOptions
            {
                Authority = UserSettings.Production ? "https://login.mhelpdesk.com/" : "https://preprod-login.mhelpdesk.com/",
                ClientId = UserSettings.ApiKey,
                ClientSecret = UserSettings.ApiSecret,
                RedirectUri = string.Format("http://127.0.0.1:54321/"),
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
                Scope = "openid profile offline_access mhdapi",
                FilterClaims = true,
                LoadProfile = true,
                Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                Policy = new Policy()
                {
                    Discovery = new IdentityModel.Client.DiscoveryPolicy
                    {
                        ValidateIssuerName = false
                    }
                }
            };

            var client = new OidcClient(options);

            return await Task.Run<AuthenticationInfo>(() =>
            {
                AuthenticationInfo newAuthInfo;
                try
                {
                    var result = client.RefreshTokenAsync(refreshToken).GetAwaiter().GetResult();
                    newAuthInfo = new AuthenticationInfo
                    {
                        AccessToken = result.AccessToken,
                        AccessTokenExpiration = result.AccessTokenExpiration,
                        RefreshToken = result.RefreshToken,
                        ApiKey = options.ClientId,
                    };
                }
                catch (Exception)
                {
                    newAuthInfo = null;
                }

                SaveAuthToFile(newAuthInfo);

                return newAuthInfo;
            });
        }

        public bool IsAuthValid(AuthenticationInfo authInfo)
        {
            return authInfo != null && authInfo.AccessTokenExpiration > DateTime.Now && authInfo.ApiKey == UserSettings.ApiKey;
        }

        public bool IsAuthRenewable(AuthenticationInfo authInfo)
        {
            return authInfo != null && 
                authInfo.RefreshToken != null && 
                authInfo.RefreshToken.Length > 0 &&
                authInfo.ApiKey == UserSettings.ApiKey;
        }

        public void SaveAuthToFile(AuthenticationInfo authInfo)
        {
            if (authInfo == null || 
                authInfo.AccessToken == null || 
                authInfo.RefreshToken == null || 
                authInfo.AccessTokenExpiration == null)
            {
                File.Delete(authFileLocation);
                return;
            }

            EncryptedFileManager.WriteObject(authInfo, authFileLocation);
            File.SetAttributes(authFileLocation, File.GetAttributes(authFileLocation) | FileAttributes.Hidden);
        }

        private async Task<AuthenticationInfo> ReadAuthFromFile()
        {
            AuthenticationInfo authInfo;

            try
            {
                authInfo = await Task.Run(() => {
                    try
                    {
                        return EncryptedFileManager.ReadObject<AuthenticationInfo>(authFileLocation);
                    }
                    catch
                    {
                        return null;
                    }
                });
            }
            catch(IOException)
            {
                authInfo = null;
            }

            return authInfo;
        }
    }
}
