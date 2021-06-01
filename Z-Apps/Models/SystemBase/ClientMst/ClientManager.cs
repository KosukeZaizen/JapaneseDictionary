using System.Collections.Generic;

namespace Z_Apps.Models.SystemBase
{
    public class ClientManager
    {
        public IEnumerable<Client> GetAllClients()
        {
            //SQL文作成
            string sql = $@"
 SELECT *
  FROM tblClientMst
";

            var dics = new DBCon().ExecuteSelect(sql, null);

            var result = new List<Client>();
            foreach (var dic in dics)
            {
                var record = new Client();
                record.userId = (string)dic["userId"];
                record.userName = (string)dic["userName"];
                record.isAdmin = (bool)dic["isAdmin"];

                result.Add(record);
            }
            return result;
        }
    }
}
