using System.Collections;
using System.Collections.Generic;
using System.Data;
using BaiRong.Core;
using BaiRong.Core.Data;
using BaiRong.Core.Model;
using SiteServer.CMS.Model;
using SiteServer.Plugin.Models;

namespace SiteServer.CMS.Provider
{
    public class PhotoDao : DataProviderBase
    {
        public override string TableName => "siteserver_Photo";

        public override List<TableColumnInfo> TableColumns => new List<TableColumnInfo>
        {
            new TableColumnInfo
            {
                ColumnName = nameof(PhotoInfo.Id),
                DataType = DataType.Integer,
                IsIdentity = true,
                IsPrimaryKey = true
            },
            new TableColumnInfo
            {
                ColumnName = nameof(PhotoInfo.PublishmentSystemId),
                DataType = DataType.Integer
            },
            new TableColumnInfo
            {
                ColumnName = nameof(PhotoInfo.ContentId),
                DataType = DataType.Integer
            },
            new TableColumnInfo
            {
                ColumnName = nameof(PhotoInfo.SmallUrl),
                DataType = DataType.VarChar,
                Length = 200
            },
            new TableColumnInfo
            {
                ColumnName = nameof(PhotoInfo.MiddleUrl),
                DataType = DataType.VarChar,
                Length = 200
            },
            new TableColumnInfo
            {
                ColumnName = nameof(PhotoInfo.LargeUrl),
                DataType = DataType.VarChar,
                Length = 200
            },
            new TableColumnInfo
            {
                ColumnName = nameof(PhotoInfo.Taxis),
                DataType = DataType.Integer
            },
            new TableColumnInfo
            {
                ColumnName = nameof(PhotoInfo.Description),
                DataType = DataType.VarChar,
                Length = 255
            }
        };

        private const string SqlUpdatePhotoContent = "UPDATE siteserver_Photo SET SmallUrl = @SmallUrl, MiddleUrl = @MiddleUrl, LargeUrl = @LargeUrl, Taxis = @Taxis, Description = @Description WHERE ID = @ID";
        private const string SqlDeletePhotoContent = "DELETE FROM siteserver_Photo WHERE ID = @ID";
        private const string SqlDeletePhotoContents = "DELETE FROM siteserver_Photo WHERE PublishmentSystemID = @PublishmentSystemID AND ContentID = @ContentID";

        private const string ParmId = "@ID";
        private const string ParmPublishmentsystemid = "@PublishmentSystemID";
        private const string ParmContentid = "@ContentID";
        private const string ParmSmallUrl = "@SmallUrl";
        private const string ParmMiddleUrl = "@MiddleUrl";
        private const string ParmLargeUrl = "@LargeUrl";
        private const string ParmTaxis = "@Taxis";
        private const string ParmDescription = "@Description";

        public void Insert(PhotoInfo photoInfo)
        {
            var maxTaxis = GetMaxTaxis(photoInfo.PublishmentSystemId, photoInfo.ContentId);
            photoInfo.Taxis = maxTaxis + 1;

            var sqlString = "INSERT INTO siteserver_Photo (PublishmentSystemID, ContentID, SmallUrl, MiddleUrl, LargeUrl, Taxis, Description) VALUES (@PublishmentSystemID, @ContentID, @SmallUrl, @MiddleUrl, @LargeUrl, @Taxis, @Description)";

            var parms = new IDataParameter[]
			{
				GetParameter(ParmPublishmentsystemid, DataType.Integer, photoInfo.PublishmentSystemId),
                GetParameter(ParmContentid, DataType.Integer, photoInfo.ContentId),
                GetParameter(ParmSmallUrl, DataType.VarChar, 200, photoInfo.SmallUrl),
                GetParameter(ParmMiddleUrl, DataType.VarChar, 200, photoInfo.MiddleUrl),
                GetParameter(ParmLargeUrl, DataType.VarChar, 200, photoInfo.LargeUrl),
                GetParameter(ParmTaxis, DataType.Integer, photoInfo.Taxis),
				GetParameter(ParmDescription, DataType.VarChar, 255, photoInfo.Description)
			};

            ExecuteNonQuery(sqlString, parms);
        }

        public void Update(PhotoInfo photoInfo)
        {
            var updateParms = new IDataParameter[]
			{
                GetParameter(ParmSmallUrl, DataType.VarChar, 200, photoInfo.SmallUrl),
                GetParameter(ParmMiddleUrl, DataType.VarChar, 200, photoInfo.MiddleUrl),
                GetParameter(ParmLargeUrl, DataType.VarChar, 200, photoInfo.LargeUrl),
                GetParameter(ParmTaxis, DataType.Integer, photoInfo.Taxis),
				GetParameter(ParmDescription, DataType.VarChar, 255, photoInfo.Description),
                GetParameter(ParmId, DataType.Integer, photoInfo.Id),
			};

            ExecuteNonQuery(SqlUpdatePhotoContent, updateParms);
        }

        public void Delete(int id)
        {
            var parms = new IDataParameter[]
			{
                GetParameter(ParmId, DataType.Integer, id)
			};

            ExecuteNonQuery(SqlDeletePhotoContent, parms);
        }

        public void Delete(List<int> idList)
        {
            if (idList != null && idList.Count > 0)
            {
                string sqlString =
                    $"DELETE FROM siteserver_Photo WHERE ID IN ({TranslateUtils.ToSqlInStringWithoutQuote(idList)})";
                ExecuteNonQuery(sqlString);
            }
        }

        public void Delete(int publishmentSystemId, int contentId)
        {
            var parms = new IDataParameter[]
			{
				GetParameter(ParmPublishmentsystemid, DataType.Integer, publishmentSystemId),
                GetParameter(ParmContentid, DataType.Integer, contentId)
			};

            ExecuteNonQuery(SqlDeletePhotoContents, parms);
        }

        public PhotoInfo GetPhotoInfo(int id)
        {
            PhotoInfo photoInfo = null;

            string sqlString =
                $"SELECT ID, PublishmentSystemID, ContentID, SmallUrl, MiddleUrl, LargeUrl, Taxis, Description FROM siteserver_Photo WHERE ID = {id}";

            using (var rdr = ExecuteReader(sqlString))
            {
                if (rdr.Read())
                {
                    var i = 0;
                    photoInfo = new PhotoInfo(GetInt(rdr, i++), GetInt(rdr, i++), GetInt(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetInt(rdr, i++), GetString(rdr, i));
                }
                rdr.Close();
            }

            return photoInfo;
        }

        public PhotoInfo GetFirstPhotoInfo(int publishmentSystemId, int contentId)
        {
            PhotoInfo photoInfo = null;

            //string sqlString =
            //    $"SELECT TOP 1 ID, PublishmentSystemID, ContentID, SmallUrl, MiddleUrl, LargeUrl, Taxis, Description FROM siteserver_Photo WHERE PublishmentSystemID = {publishmentSystemId} AND ContentID = {contentId} ORDER BY Taxis";
            var sqlString = SqlUtils.ToTopSqlString("siteserver_Photo", "ID, PublishmentSystemID, ContentID, SmallUrl, MiddleUrl, LargeUrl, Taxis, Description", $"WHERE PublishmentSystemID = {publishmentSystemId} AND ContentID = {contentId}", "ORDER BY Taxis", 1);

            using (var rdr = ExecuteReader(sqlString))
            {
                if (rdr.Read())
                {
                    var i = 0;
                    photoInfo = new PhotoInfo(GetInt(rdr, i++), GetInt(rdr, i++), GetInt(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetInt(rdr, i++), GetString(rdr, i));
                }
                rdr.Close();
            }

            return photoInfo;
        }

        public int GetCount(int publishmentSystemId, int contentId)
        {
            string sqlString =
                $"SELECT Count(*) FROM siteserver_Photo WHERE PublishmentSystemID = {publishmentSystemId} AND ContentID = {contentId}";

            return BaiRongDataProvider.DatabaseDao.GetIntResult(sqlString);
        }

        public string GetSortFieldName()
        {
            return "Taxis";
        }

        public string GetSelectSqlString(int publishmentSystemId, int contentId)
        {
            return
                $"SELECT ID, PublishmentSystemID, ContentID, SmallUrl, MiddleUrl, LargeUrl, Taxis, Description FROM siteserver_Photo WHERE PublishmentSystemID = {publishmentSystemId} AND ContentID = {contentId} ORDER BY Taxis";
        }

        public IEnumerable GetStlDataSource(int publishmentSystemId, int contentId, int startNum, int totalNum)
        {
            var tableName = "siteserver_Photo";
            var orderByString = "ORDER BY Taxis";
            string whereString = $"WHERE (PublishmentSystemID = {publishmentSystemId} AND ContentID = {contentId})";

            var sqlSelect = BaiRongDataProvider.DatabaseDao.GetSelectSqlString(tableName, startNum, totalNum, SqlUtils.Asterisk, whereString, orderByString);

            return (IEnumerable)ExecuteReader(sqlSelect);
        }

        public List<int> GetPhotoContentIdList(int publishmentSystemId, int contentId)
        {
            var list = new List<int>();

            string sqlString =
                $"SELECT ID FROM siteserver_Photo WHERE PublishmentSystemID = {publishmentSystemId} AND ContentID = {contentId} ORDER BY Taxis";

            using (var rdr = ExecuteReader(sqlString))
            {
                while (rdr.Read())
                {
                    list.Add(GetInt(rdr, 0));
                }
                rdr.Close();
            }

            return list;
        }

        public List<PhotoInfo> GetPhotoInfoList(int publishmentSystemId, int contentId)
        {
            var list = new List<PhotoInfo>();

            string sqlString =
                $"SELECT ID, PublishmentSystemID, ContentID, SmallUrl, MiddleUrl, LargeUrl, Taxis, Description FROM siteserver_Photo WHERE PublishmentSystemID = {publishmentSystemId} AND ContentID = {contentId} ORDER BY Taxis";

            using (var rdr = ExecuteReader(sqlString))
            {
                while (rdr.Read())
                {
                    var i = 0;
                    list.Add(new PhotoInfo(GetInt(rdr, i++), GetInt(rdr, i++), GetInt(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetInt(rdr, i++), GetString(rdr, i)));
                }
                rdr.Close();
            }

            return list;
        }

        private int GetTaxis(int id)
        {
            string sqlString = $"SELECT Taxis FROM siteserver_Photo WHERE (ID = {id})";

            return BaiRongDataProvider.DatabaseDao.GetIntResult(sqlString);
        }

        private void SetTaxis(int id, int taxis)
        {
            string sqlString = $"UPDATE siteserver_Photo SET Taxis = {taxis} WHERE (ID = {id})";
            ExecuteNonQuery(sqlString);
        }

        private int GetMaxTaxis(int publishmentSystemId, int contentId)
        {
            string sqlString =
                $"SELECT MAX(Taxis) FROM siteserver_Photo WHERE (PublishmentSystemID = {publishmentSystemId} AND ContentID = {contentId})";
            var maxTaxis = 0;

            using (var rdr = ExecuteReader(sqlString))
            {
                if (rdr.Read())
                {
                    maxTaxis = GetInt(rdr, 0);
                }
                rdr.Close();
            }
            return maxTaxis;
        }

        public bool UpdateTaxisToUp(int publishmentSystemId, int contentId, int id)
        {
            //Get Higher Taxis and ID
            //string sqlString =
            //    $"SELECT TOP 1 ID, Taxis FROM siteserver_Photo WHERE (Taxis > (SELECT Taxis FROM siteserver_Photo WHERE ID = {id}) AND (PublishmentSystemID = {publishmentSystemId} AND ContentID = {contentId})) ORDER BY Taxis";
            string sqlString = SqlUtils.ToTopSqlString("siteserver_Photo", "ID, Taxis", $"WHERE (Taxis > (SELECT Taxis FROM siteserver_Photo WHERE ID = {id}) AND (PublishmentSystemID = {publishmentSystemId} AND ContentID = {contentId}))", "ORDER BY Taxis", 1);

            var higherId = 0;
            var higherTaxis = 0;

            using (var rdr = ExecuteReader(sqlString))
            {
                if (rdr.Read())
                {
                    higherId = GetInt(rdr, 0);
                    higherTaxis = GetInt(rdr, 1);
                }
                rdr.Close();
            }

            if (higherId > 0)
            {
                //Get Taxis Of Selected ID
                var selectedTaxis = GetTaxis(id);

                //Set The Selected Class Taxis To Higher Level
                SetTaxis(id, higherTaxis);
                //Set The Higher Class Taxis To Lower Level
                SetTaxis(higherId, selectedTaxis);
                return true;
            }
            return false;
        }

        public bool UpdateTaxisToDown(int publishmentSystemId, int contentId, int id)
        {
            //Get Lower Taxis and ID
            //string sqlString =
            //    $"SELECT TOP 1 ID, Taxis FROM siteserver_Photo WHERE (Taxis < (SELECT Taxis FROM siteserver_Photo WHERE ID = {id}) AND (PublishmentSystemID = {publishmentSystemId} AND ContentID = {contentId})) ORDER BY Taxis DESC";
            var sqlString = SqlUtils.ToTopSqlString("siteserver_Photo", "ID, Taxis", $"WHERE (Taxis < (SELECT Taxis FROM siteserver_Photo WHERE ID = {id}) AND (PublishmentSystemID = {publishmentSystemId} AND ContentID = {contentId}))", "ORDER BY Taxis DESC", 1);

            var lowerId = 0;
            var lowerTaxis = 0;

            using (var rdr = ExecuteReader(sqlString))
            {
                if (rdr.Read())
                {
                    lowerId = GetInt(rdr, 0);
                    lowerTaxis = GetInt(rdr, 1);
                }
                rdr.Close();
            }

            if (lowerId > 0)
            {
                //Get Taxis Of Selected Class
                var selectedTaxis = GetTaxis(id);

                //Set The Selected Class Taxis To Lower Level
                SetTaxis(id, lowerTaxis);
                //Set The Lower Class Taxis To Higher Level
                SetTaxis(lowerId, selectedTaxis);
                return true;
            }
            return false;
        }

    }
}