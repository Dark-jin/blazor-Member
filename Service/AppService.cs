using Member.Model;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Member.Service
{
    public class AppService : IAppService
    {
        public async Task<MainResponse> AuthenticateUser(LoginModel loginModel)
        {
            var returnResponse = new MainResponse();
            using (var client = new HttpClient())
            {
                var url = $"{Setting.BaseUrl}{APIs.AuthenticateUser}";
                var serializedStr = JsonConvert.SerializeObject(loginModel); // .NET 객체로부터 JSON 문자열을 만들기 위함

                var response = await client.PostAsync(url, new StringContent(serializedStr, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    // ReadAsStringAsync() : HTTP 콘텐츠를 비동기 작업으로 문자열로 serialize합니다.(직렬화)
                    string contentStr = await response.Content.ReadAsStringAsync();
                    returnResponse = JsonConvert.DeserializeObject<MainResponse>(contentStr); // JSON 문자열로부터 .NET 객체를 다시 복원하기 위함
                    returnResponse.Content = contentStr;
                    returnResponse.IsSuccess = true;
                }
                else
                {
                    returnResponse.ErrorMessage = "Authentication failed.";
                    returnResponse.IsSuccess = false;
                }
            }
            return returnResponse;
        }

        public async Task<StudentModel> UserProfile()
        {
            var returnResponse = new StudentModel();
            using (var client = new HttpClient())
            {
                var url = $"{Setting.BaseUrl}{APIs.UserProfile}";
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Setting.UserBasicDetail?.AccessToken}");

                var response = await client.GetAsync(url);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    bool isTokenRefreshed = await RefreshToken();
                    if (isTokenRefreshed) return await UserProfile();
                }
                else
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string contentStr = await response.Content.ReadAsStringAsync();
                        var mainResponse = JsonConvert.DeserializeObject<MainResponse>(contentStr);
                        if (response.IsSuccessStatusCode == false)
                        {
                            mainResponse.ErrorMessage = await response.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            mainResponse.IsSuccess = true;
                            mainResponse.Content = contentStr;
                        }
                        if (mainResponse.IsSuccess)
                        {
                            returnResponse = JsonConvert.DeserializeObject<StudentModel>(mainResponse.Content.ToString());
                        }
                    }
                }
            }
            return returnResponse;
        }

        public async Task<bool> RefreshToken()
        {
            bool isTokenRefreshed = false;
            using (var client = new HttpClient())
            {
                var url = $"{Setting.BaseUrl}{APIs.RefreshToken}";

                var serializedStr = JsonConvert.SerializeObject(new AuthenticateRequestAndResponse
                {
                    RefreshToken = Setting.UserBasicDetail.RefreshToken,
                    AccessToken = Setting.UserBasicDetail.AccessToken
                });

                try
                {
                    var response = await client.PostAsync(url, new StringContent (serializedStr, Encoding.UTF8, "application/json"));
                    if(response.IsSuccessStatusCode)
                    {
                        string contentStr = await response.Content.ReadAsStringAsync();
                        var mainResponse = JsonConvert.DeserializeObject<MainResponse>(contentStr);
                        if (mainResponse.IsSuccess)
                        {
                            var tokenDetails = JsonConvert.DeserializeObject<AuthenticateRequestAndResponse>(mainResponse.Content.ToString());
                            Setting.UserBasicDetail.AccessToken = tokenDetails.AccessToken;
                            Setting.UserBasicDetail.RefreshToken = tokenDetails.RefreshToken;

                            string userDetailsStr = JsonConvert.SerializeObject(Setting.UserBasicDetail);
                            // AccessToken, RefreshToken을 SetAsync() 메서드를 통해 보안 저장소에 업데이트
                            await SecureStorage.SetAsync(nameof(Setting.UserBasicDetail), userDetailsStr);
                            isTokenRefreshed = true;
                        }
                    }
                }
                catch(Exception e)
                {
                    string msg = e.Message;
                }
            }
            return isTokenRefreshed;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> RegisterUser(RegistrationModel registerUser)
        {
            string errorMessage = string.Empty;
            bool isSuccess = false;

            using(var client = new HttpClient())
            {
                var url = $"{Setting.BaseUrl}{APIs.RegisterUser}";

                var serializedStr = JsonConvert.SerializeObject(registerUser);
                var response = await client.PostAsync(url, new StringContent(serializedStr, Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    isSuccess = true;
                }
                else
                {
                    errorMessage = await response.Content.ReadAsStringAsync();
                }
            }

            return (isSuccess, errorMessage);
        }
        public async Task<MainResponse> UserLogout()
        {
            using (var client = new HttpClient())
            {
                var url = $"{Setting.BaseUrl}{APIs.UserLogout}";

                // Authorize
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Setting.UserBasicDetail?.AccessToken}");

                var queryParameter = new Dictionary<string, string>
                {
                    { "refresh", Setting.UserBasicDetail.RefreshToken }
                };
                var requestUri = new Uri(QueryHelpers.AddQueryString(url, queryParameter));
                var jsonContent = new StringContent(JsonConvert.SerializeObject(queryParameter), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(requestUri, jsonContent);

                // SecueStorage에 저장되어있는 토큰 모두 삭제
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var mainResponse = JsonConvert.DeserializeObject<MainResponse>(responseContent);
                    SecureStorage.RemoveAll();
                    return mainResponse;
                }
                else
                {
                    // 에러 응답 처리
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Logout erro : {errorContent}");
                }
            }
        }
    }
}
