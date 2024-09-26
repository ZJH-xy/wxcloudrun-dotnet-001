namespace aspnetapp.Dao.RepositoryInterface.Web {
    /// <summary>
    /// 批量获取表信息接口
    /// </summary>
    public interface IRepositoryWeb {
        /// <summary>
        /// 批量获取表信息
        /// </summary>
        /// <param name="iimit">用于分页控制，表示返回多少行数据</param>
        /// <param name="offset">表示跳过前面多少条记录（即从哪一条记录开始）</param>
        /// <param name="includeStructure">是否返回表结构</param>
        /// <param name="includeData">是否返回表数据</param>
        /// <returns></returns>
        IQueryable<Object> GetTableData(int iimit, int offset, bool includeStructure = true, bool includeData = true);
    }
}
