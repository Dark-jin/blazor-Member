using Member.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Member.Service
{
    public interface IAppService
    {
        // 인증 토큰이 성공적으로 리프레쉬됐는지 여부를 나타내는 bool을 반환하는 메서드
        Task<bool> RefreshToken();
        // 객체를 가져와 사용자가 성공적으로 인증되면 인증 토큰과 사용자 세부 정보가 포함된 객체를 LoginModel에 반환하는 메서드
        public Task<MainResponse> AuthenticateUser(LoginModel loginModel);
        // 객체를 가져와 사용자 등록 성공 여부를 나타내는 bool 값과 해당하는 경우 오류 메시지가 포함된 튜플을 반환하는 메서드
        Task<(bool IsSuccess, string ErrorMessage)> RegisterUser(RegistrationModel registerUser);
        // 사용자에 대한 정보가 포함된 개체 목록을 반환하는 메서드
        Task<StudentModel> UserProfile();
        // 유저 로그아웃 메서드
        Task <MainResponse> UserLogout(UserBasicDetail userBasicDetail);
    }
}
