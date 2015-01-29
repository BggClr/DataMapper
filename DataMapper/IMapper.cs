namespace DataMapper
{
    internal interface IMapper
    {
        TDest Map<TSource, TDest>(TSource source);
    }
}