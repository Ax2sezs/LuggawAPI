namespace backend.Models
{
    public class PosCouponResponse
    {
        public string rw_transection_id { get; set; }
        public int rw_type_receive { get; set; }
        public int rw_member_id { get; set; }
        public Guid mem_guid { get; set; }
        public string rw_rewardstatus { get; set; }
        public string mem_number { get; set; }
        public string mem_firstname { get; set; }
        public string mem_lastname { get; set; }
        public string mem_phone { get; set; }
        public string mem_email { get; set; }
        public string rw_branch_id { get; set; }
        public string? b_code { get; set; }
        public string b_name { get; set; }
        public string b_address { get; set; }
        public string rw_member_name { get; set; }
        public string rw_member_phone { get; set; }
        public string rw_member_address { get; set; }
        public string rw_member_district { get; set; }
        public string rw_member_amphoe { get; set; }
        public string rw_member_province { get; set; }
        public string rw_member_zipcode { get; set; }
        public DateTime? rw_burn_date { get; set; }
        public string rw_expired_at { get; set; }
        public string rw_type_receive_name { get; set; }
        public List<PosProductItem> product { get; set; }
    }

}
