namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IHashingService
    {
        public string Hashing(string value, string key);

        public string DeHashing(string value, string key);
    }
}