namespace WebApplication.Models
{
    public class Messenger
    {
        private object _data;
        
        public string Message { get; set; }

        public bool IsError { get; set; }

        public Messenger(string message, bool isError)
        {
            this.Message = message;
            this.IsError = isError;
        }

        public T GetData<T>()
        {
            return (T) _data;
        }

        public void SetData<T>(T data)
        {
            this._data = data;
        }
        
    }
}