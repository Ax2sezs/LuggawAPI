namespace backend.Models
{
    public class PosProductItem
    {
        public int rw_reward_id { get; set; }
        public Guid rw_reward_guid { get; set; }
        public string rw_rewardcode { get; set; }
        public string rewards_channel_code { get; set; }
        public string rewards_name_th { get; set; }
        public DateTime rewards_start { get; set; }
        public DateTime rewards_end { get; set; }
        public string rewards_discount_type { get; set; }
        public string rewards_amount_min { get; set; }
        public string rewards_discount_max { get; set; }
        public string rewards_discount_percent { get; set; }
        public string rewards_category_name { get; set; }
        public int rw_pointperunit { get; set; }
        public int rw_count { get; set; }
        public int totalPoint { get; set; }
        public string rw_rewardstatus { get; set; }
    }

}
