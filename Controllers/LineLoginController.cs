using Microsoft.AspNetCore.Mvc;
using backend.Models;
using LineLoginBackend.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using backend.Service;
using System.Security.Claims;
using backend.Service.Interfaces;
using Newtonsoft.Json.Linq;
using System.Data;
using backend.Utils;

namespace LineLoginBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LineLoginController : ControllerBase
    {
        private readonly ILineLoginService _lineLoginService;
        private readonly AppDbContext _dbContext;
        private readonly IOtpService _otpService;

        public LineLoginController(ILineLoginService lineLoginService, AppDbContext dbContext, IOtpService otpService)
        {
            _lineLoginService = lineLoginService;
            _dbContext = dbContext;
            _otpService = otpService;
        }

        [HttpPost("AddMember")]
        public async Task<IActionResult> AddMember([FromBody] AddMemberRequest model)
        {
            JObject result = new JObject();
            var keyencrypt = EnvironmentConfiguration.GetSingleton().GetKeyEnCrypt;
            var list = new List<string>();

            try
            {
                string phone_encrpt = EnvironmentConfiguration.GetSingleton().Encrypt256(model.MB_Tel);

                list.Add(model.MB_Name);
                list.Add(model.MB_Lastname);
                var birthDay = DateTime.Parse(model.MB_Birthday).ToString("yyyy-MM-dd");
                list.Add(birthDay);
                list.Add(model.MB_IdCard);
                list.Add(model.MB_Tel);
                list.Add(model.MB_Email);
                list.Add(model.MB_Address);
                list.Add(string.IsNullOrEmpty(model.MB_Tumbol_Id) ? "00000000-0000-0000-0000-000000000000" : model.MB_Tumbol_Id);
                list.Add(string.IsNullOrEmpty(model.MB_Amphur_Id) ? "00000000-0000-0000-0000-000000000000" : model.MB_Amphur_Id);
                list.Add(string.IsNullOrEmpty(model.MB_Province_Id) ? "00000000-0000-0000-0000-000000000000" : model.MB_Province_Id);
                list.Add(model.MB_Postcode);
                list.Add(model.MB_Password);
                list.Add(string.IsNullOrEmpty(model.CreatedBy) ? "00000000-0000-0000-0000-000000000000" : model.CreatedBy);
                list.Add(string.IsNullOrEmpty(model.VersionNumber) ? "00000000-0000-0000-0000-000000000000" : model.VersionNumber);
                list.Add(model.MB_Sex);
                list.Add(model.MB_AgeRang);
                list.Add(keyencrypt);
                list.Add(phone_encrpt);
                list.Add("POS");
            }
            catch (Exception e)
            {
                result["isSuccess"] = false;
                result["Status"] = "0";
                result["Message"] = $"Please input all field : {e.Message}";
                result["Data"] = null;
                return Ok(result);
            }

            try
            {
                result = await Connection.GetSingleton()
                    .ExcuteStoredProcedureThatRespondSelectIntoJsonObject(
                        "CreateNewMember",
                        list,
                        CommandType.StoredProcedure,
                        "Add new member success"
                    );

                JObject final = JObject.FromObject(result["Data"][0]);
                result["isSuccess"] = true;
                result["Data"] = final;
            }
            catch
            {
                result["isSuccess"] = false;
                result["Data"] = null;
            }

            return Ok(result);
        }


        //[HttpGet("me")]
        //[Authorize]
        //public async Task<IActionResult> GetMyProfile()
        //{
        //    try
        //    {
        //        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(userId))
        //            return Unauthorized();

        //        var user = await _lineLoginService.GetUserByIdAsync(userId);
        //        if (user == null)
        //            return NotFound();

        //        return Ok(user);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserRequest request)
        {
            var success = await _lineLoginService.UpdateUserAsync(userId, request);
            if (!success)
                return NotFound("User not found");

            return Ok("User updated successfully");
        }


        //[HttpGet("callback")]
        //[AllowAnonymous]
        //public async Task<IActionResult> Callback([FromQuery] string code)
        //{
        //    try
        //    {
        //        Console.WriteLine($"Received code: {code}");
        //        if (string.IsNullOrEmpty(code))
        //            return BadRequest(new { error = "Code parameter is missing" });

        //        var token = await _lineLoginService.ExchangeCodeForTokenAsync(code);
        //        var profile = await _lineLoginService.GetUserProfileAsync(token.AccessToken);

        //        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.LineUserId == profile.UserId);

        //        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
        //            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

        //        if (user == null)
        //        {
        //            user = new User
        //            {
        //                LineUserId = profile.UserId,
        //                DisplayName = profile.DisplayName,
        //                PictureUrl = profile.PictureUrl,
        //                PhoneNumber = profile.PhoneNumber,
        //                BirthDate = profile.BirthDate,
        //                Gender = profile.Gender,
        //                CreatedAt = now,
        //                IsActive = true,
        //                IsCompleted = false,
        //            };
        //            _dbContext.Users.Add(user);
        //            await _dbContext.SaveChangesAsync();
        //        }
        //        else
        //        {
        //            if (!user.IsActive)
        //            if (!user.IsActive)
        //            {
        //                return Unauthorized(new { error = "This Account is Disabled." });
        //            }

        //            user.DisplayName = profile.DisplayName;
        //            user.PictureUrl = profile.PictureUrl;
        //            await _dbContext.SaveChangesAsync();
        //        }

        //        // ✅ ใช้ UserId (Guid) แทน LineUserId
        //        var jwt = _lineLoginService.GenerateJwtToken(user.UserId, user.DisplayName);

        //        return Ok(new
        //        {
        //            token = jwt,
        //            user.UserId,
        //            user.LineUserId,
        //            user.DisplayName,
        //            user.PictureUrl,
        //            user.PhoneNumber,
        //            user.BirthDate,
        //            user.Gender,
        //            user.IsActive,
        //            user.IsCompleted,
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error in callback: " + ex.Message);
        //        return BadRequest(new { error = ex.Message });
        //    }
        //}

        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            var logger = new LoginLogger();

            try
            {
                Console.WriteLine($"Received code: {code}");
                if (string.IsNullOrEmpty(code))
                {
                    await logger.WriteLogAsync("Login","❌ Login failed: Missing code");
                    return BadRequest(new { error = "Code parameter is missing" });
                }

                var token = await _lineLoginService.ExchangeCodeForTokenAsync(code);
                var profile = await _lineLoginService.GetUserProfileAsync(token.AccessToken);

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.LineUserId == profile.UserId);

                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                if (user == null)
                {
                    var tempUser = new User
                    {
                        LineUserId = profile.UserId,
                        DisplayName = profile.DisplayName,
                        PictureUrl = profile.PictureUrl,
                        PhoneNumber = profile.PhoneNumber,
                        BirthDate = profile.BirthDate,
                        Gender = profile.Gender,
                        CreatedAt = now,
                        IsActive = true,
                        IsCompleted = false,
                    };

                    _dbContext.Users.Add(tempUser);
                    await _dbContext.SaveChangesAsync();

                    user = tempUser;

                    await logger.WriteLogAsync("Login",$"🆕 New user registered: {user.PhoneNumber} ({user.UserId})");
                }
                else
                {
                    if (!user.IsActive)
                    {
                        await logger.WriteLogAsync("Login",$"⛔ Blocked login: {user.PhoneNumber} ({user.UserId})");
                        return Unauthorized(new { error = "This Account is Disabled." });
                    }

                    user.DisplayName = profile.DisplayName;
                    user.PictureUrl = profile.PictureUrl;

                    if (string.IsNullOrEmpty(user.MemberNumber))
                    {
                        var random = new Random();
                        string memberNumber = string.Concat(Enumerable.Range(0, 16).Select(_ => random.Next(0, 10).ToString()));
                        user.MemberNumber = memberNumber;
                    }
                    await _dbContext.SaveChangesAsync();

                    await logger.WriteLogAsync("Login",$"✅ Login success: {user.PhoneNumber} ({user.UserId})");
                }

                var jwt = _lineLoginService.GenerateJwtToken(user.UserId, user.LineUserId, null);

                return Ok(new
                {
                    token = jwt,
                    user.DisplayName,
                    user.FirstName,
                    user.LastName,
                    user.PictureUrl,
                    user.PhoneNumber,
                    user.BirthDate,
                    user.Gender,
                    user.IsActive,
                    user.IsCompleted,
                });
            }
            catch (Exception ex)
            {
                await new LoginLogger().WriteLogAsync("Login",$"🔥 Error in callback: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        private bool IsUserReadyForPOS(User user)
        {
            return !string.IsNullOrWhiteSpace(user.PhoneNumber)
                && user.BirthDate != null
                && !string.IsNullOrWhiteSpace(user.Gender)
                && !string.IsNullOrWhiteSpace(user.UserId.ToString())
                && !string.IsNullOrWhiteSpace(user.FirstName)
                && !string.IsNullOrWhiteSpace(user.LastName)
                && user.UserId != Guid.Empty;
        }



        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _dbContext.Users
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("users/{userId}/points")]
        public async Task<IActionResult> GetUserPoints(Guid userId)
        {
            var userPoint = await _dbContext.UserPoints
                .FirstOrDefaultAsync(up => up.User.UserId == userId);

            if (userPoint == null) return NotFound();

            return Ok(new { point = userPoint.TotalPoints });
        }

        private bool IsUserProfileComplete(User user)
        {
            return !string.IsNullOrWhiteSpace(user.PhoneNumber)
                && user.BirthDate != null
                && !string.IsNullOrWhiteSpace(user.Gender);
        }



        [Authorize]
        [HttpGet("check-profile")]
        public async Task<IActionResult> CheckProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid User ID in token");

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound("User not found");

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "User is inactive" });
            }

            return Ok(new
            {
                hasProfile = user.IsCompleted
            });
        }





        //[Authorize]
        //[HttpPost("complete-profile")]
        //public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileRequest request)
        //{
        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (string.IsNullOrEmpty(userIdClaim))
        //        return Unauthorized("User ID not found in token");

        //    if (!Guid.TryParse(userIdClaim, out var userId))
        //        return Unauthorized("Invalid User ID in token");

        //    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        //    if (user == null) return NotFound("User not found");

        //    user.PhoneNumber = request.PhoneNumber;
        //    user.BirthDate = request.BirthDate;
        //    user.Gender = request.Gender;
        //    user.IsCompleted = request.IsCompleted;

        //    await _dbContext.SaveChangesAsync();

        //    return Ok();
        //}

        [Authorize]
        [HttpPost("complete-profile")]
        public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileRequest request)
        {
            var logger = new LoginLogger();

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                await logger.WriteLogAsync("CompleteProfile","❌ CompleteProfile failed: Invalid User ID in token");
                return Unauthorized("Invalid User ID in token");
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                await logger.WriteLogAsync("CompleteProfile",$"❌ CompleteProfile failed: User not found ({userId})");
                return NotFound("User not found");
            }

            var isChangingPhone = !string.IsNullOrEmpty(user.PhoneNumber)
                                  && user.PhoneNumber != request.PhoneNumber;

            if (string.IsNullOrEmpty(user.PhoneNumber) || isChangingPhone)
            {
                var duplicatePhone = await _dbContext.PhoneNumbers
                    .AnyAsync(p => p.Phone_Number == request.PhoneNumber && p.IsActive);

                if (duplicatePhone)
                {
                    await logger.WriteLogAsync("CompleteProfile",$"⚠️ Phone duplicate: {request.PhoneNumber} (UserId={userId})");
                    return BadRequest("เบอร์โทรนี้ถูกใช้แล้ว กรุณาใช้เบอร์อื่น");
                }

                var oldPhones = await _dbContext.PhoneNumbers
                    .Where(p => p.UserId == userId && p.IsActive)
                    .ToListAsync();

                foreach (var oldPhone in oldPhones)
                {
                    oldPhone.IsActive = false;
                }

                _dbContext.PhoneNumbers.Add(new PhoneNumber
                {
                    UserId = userId,
                    Phone_Number = request.PhoneNumber!,
                    IsActive = true,
                    CreatedAt = now
                });
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.BirthDate = request.BirthDate;
            user.Gender = request.Gender;
            user.IsCompleted = true;

            var existingUserPoints = await _dbContext.UserPoints.FirstOrDefaultAsync(up => up.UserId == userId);
            if (existingUserPoints == null)
            {
                _dbContext.UserPoints.Add(new UserPoints
                {
                    UserId = userId,
                    TotalPoints = 0
                });
            }

            await _dbContext.SaveChangesAsync();

            if (IsUserReadyForPOS(user))
            {
                Console.WriteLine(">> Syncing completed profile to POS");
                await logger.WriteLogAsync("CompleteProfile",$"POS sync completed: {user.FirstName} {user.LastName} ({user.PhoneNumber})");
                await AddMemberAutomatically(user, now);
            }

            await logger.WriteLogAsync("CompleteProfile",$"CompleteProfile success: {user.FirstName} {user.LastName} ({user.PhoneNumber})");

            return Ok();
        }



        [Authorize]
        [HttpPost("update-phone")]
        public async Task<IActionResult> UpdatePhone([FromBody] EditPhoneRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid User ID in token");

            if (string.IsNullOrEmpty(request.OldPhone) || string.IsNullOrEmpty(request.NewPhone))
                return BadRequest("Missing phone data.");

            // ตรวจสอบว่าเบอร์เก่าตรงกับในระบบหรือไม่
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound("User not found");

            if (user.PhoneNumber != request.OldPhone)
                return BadRequest("เบอร์เก่าไม่ตรงกับข้อมูลในระบบ");

            (bool success, string? signature, long timestamp) = await _lineLoginService.UpdatePhoneNumberWithSignatureAsync(
                userId,
                request.NewPhone
            );

            if (!success)
                return NotFound("ไม่พบข้อมูลลูกค้าในระบบ");

            Response.Headers.Add("X-Timestamp", timestamp.ToString());
            Response.Headers.Add("X-Signature", signature ?? "");

            return Ok(new
            {
                success = true,
                phone = request.NewPhone
            });
        }
        [Authorize]
        [HttpPost("check-phone")]
        public async Task<IActionResult> CheckPhoneNumber([FromBody] CheckPhoneRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid User ID in token");

            if (string.IsNullOrEmpty(request.Phone))
                return BadRequest("Phone number is required.");

            var isUsed = await _lineLoginService.IsPhoneNumberAlreadyUsedAsync(request.Phone, userId);

            return Ok(new { isUsed });
        }
        [Authorize]
        [HttpPost("edit-profile-name")]
        public async Task<IActionResult> EditProfileName([FromBody] EditProfileDto dto)
        {
            try
            {
                // 1. Extract userId from JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized("Invalid User ID in token");

                // 2. Call Service
                var result = await _lineLoginService.EditProfileNameAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EditProfileName Error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }


        private NewMemberFromLineDto CreateDtoFromUser(User user, string encryptedPhone, string keyEncrypt)
        {
            return new NewMemberFromLineDto
            {
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                BirthDate = user.BirthDate,
                IdCard = "",
                Phone = user.PhoneNumber ?? "",
                Email = "",
                Address = "",

                TumbolId = Guid.Empty,
                AmphurId = Guid.Empty,
                ProvinceId = Guid.Empty,
                Postcode = "",
                Password = "",

                CreatedBy = Guid.Empty,
                UpdatedBy = Guid.Empty,
                Gender = user.Gender ?? "",
                AgeRange = "",

                EncryptKey = keyEncrypt ?? "",
                EncryptedPhone = encryptedPhone,
                Source = "LINE",
                ExternalUserId = user.UserId.ToString() ?? ""
            };
        }

        private List<string> ToStoredProcedureParams(NewMemberFromLineDto dto)
        {
            return new List<string>
    {
        dto.FirstName,
        dto.LastName,
        dto.BirthDate?.ToString("yyyy-MM-dd") ?? "",
        dto.IdCard,
        dto.Phone,
        dto.Email,
        dto.Address,
        dto.TumbolId.ToString(),
        dto.AmphurId.ToString(),
        dto.ProvinceId.ToString(),
        dto.Postcode,
        dto.Password,
        dto.CreatedBy.ToString(),
        dto.UpdatedBy.ToString(),
        dto.Gender,
        dto.AgeRange,
        dto.EncryptKey,
        dto.EncryptedPhone,
        dto.Source,
        dto.ExternalUserId
    };
        }

        private async Task AddMemberAutomatically(User user, DateTime createdAt)
        {
            var keyencrypt = EnvironmentConfiguration.GetSingleton().GetKeyEnCrypt;
            string phone_encrypt = EnvironmentConfiguration.GetSingleton().Encrypt256(user.PhoneNumber);

            var dto = CreateDtoFromUser(user, phone_encrypt, keyencrypt);
            var paramList = ToStoredProcedureParams(dto);

            var result = await Connection.GetSingleton().ExcuteStoredProcedureThatRespondSelectIntoJsonObject(
                "CreateNewMember",
                paramList,
                CommandType.StoredProcedure,
                "Auto Add new member from LINE"
            );
        }


        //        private async Task AddMemberAutomatically(User user, DateTime createdAt)
        //        {
        //            var keyencrypt = EnvironmentConfiguration.GetSingleton().GetKeyEnCrypt;
        //            string phone_encrypt = EnvironmentConfiguration.GetSingleton().Encrypt256(user.PhoneNumber);

        //            var list = new List<string>
        //{
        //    user.DisplayName??"",
        //    "",
        //    user.BirthDate?.ToString("yyyy-MM-dd") ?? "",
        //    "",
        //    user.PhoneNumber??"",
        //    "",
        //    "",
        //    "00000000-0000-0000-0000-000000000000",
        //    "00000000-0000-0000-0000-000000000000",
        //    "00000000-0000-0000-0000-000000000000",
        //    "",
        //    "",
        //    "00000000-0000-0000-0000-000000000000",
        //    "00000000-0000-0000-0000-000000000000",
        //    user.Gender ?? "",
        //    "",
        //    keyencrypt ?? "",
        //    phone_encrypt ?? "",
        //    "POS",
        //    user.UserId.ToString()??"",
        //};
        //            var result = 
        //            await Connection.GetSingleton().ExcuteStoredProcedureThatRespondSelectIntoJsonObject(
        //                "CreateNewMemberFromLine",
        //                list,
        //                CommandType.StoredProcedure,
        //                "Auto Add new member from LINE"
        //            );
        //        }




    }
}
