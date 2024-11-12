namespace aspnetapp.Dao.RepositoryInterface.Miniprogram {
    public interface IFavoritesStoreRepository {
        /// <summary>
        /// 获取收藏
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<UserFavoritesStore?> GetById(int id);
        /// <summary>
        /// 获取收藏
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<UserFavoritesStore>> GetByUserId(int userId);
        /// <summary>
        /// 添加收藏
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        Task<int> AddFavoritesStore(int userId, int storeId);
        /// <summary>
        /// 删除收藏
        /// </summary>
        /// <param name="favoritesStoreId"></param>
        /// <returns></returns>
        Task<int> DelFavoritesStore(int favoritesStoreId);
    }
}
