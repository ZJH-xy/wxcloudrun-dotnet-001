namespace aspnetapp.Dao.RepositoryInterface.Web {
    /// <summary>
    /// WEB共同接口
    /// </summary>
    public interface IRepositoryWeb {
        /// <summary>
        /// 根据ID 查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //Task<Object?> GetById(int id);
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="iimit">一页多少行</param>
        /// <param name="pageIndex">第几页</param>
        /// <returns></returns>
        //Task<List<Object>> GetTablePage(int iimit, int pageIndex);
        /// <summary>
        /// 获取表结构
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetTableStructure();
        /// <summary>
        /// 批量获取表信息
        /// </summary>
        /// <param name="iimit">用于分页控制，表示返回多少行数据</param>
        /// <param name="offset">表示跳过前面多少条记录（即从哪一条记录开始）</param>
        /// <param name="includeStructure">是否返回表结构</param>
        /// <param name="includeData">是否返回表数据</param>
        /// <returns></returns>
        //IQueryable<Object> GetTableData(int iimit, int offset, bool includeStructure = true, bool includeData = true);
    }
}
