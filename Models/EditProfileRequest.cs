public class EditProfileRequest
{
    public string? desc { get; set; }
    public bool? isSuccess { get; set; }
    public string? errMsg { get; set; }
    public string? data { get; set; }
    // เพิ่ม field ได้ถ้า POS ส่งกลับมาเพิ่มเติม
}
