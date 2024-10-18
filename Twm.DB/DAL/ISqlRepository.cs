namespace AlgoDesk.Model.DAL
{
    public interface ISqlRepository
    {
        #region Param      
        bool CreateOrUpdate<T>(T instance) where T : class;
        bool Create<T>(T instance) where T : class;
        bool Create<T>(T[] instances) where T : class;
        bool Update<T>(T instance) where T : class;
        bool Delete<T>(object id);

        #endregion
    }
}