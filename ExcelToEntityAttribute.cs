namespace Excel.Tool
{
    /// <summary>
    /// excel转实体
    /// </summary>
    public class ExcelToEntityAttribute : System.Attribute
    {
        public ExcelToEntityAttribute(int ColNo)
        {
            this.ColNo = ColNo;
        }
        public ExcelToEntityAttribute()
        {

        }
        public int ColNo { get; set; }
    }
    /// <summary>
    /// 实体转excel
    /// </summary>
    public class EntityToexcelAttribute : System.Attribute
    {
        public EntityToexcelAttribute(string colname,int idx)
        {
            this.Colname = colname;
            this.Idx = idx;
        }
        public string Colname { get; set; }
        public int Idx { get; set; }
    }
}
