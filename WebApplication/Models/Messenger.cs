namespace WebApplication.Models
{
    public class Messenger
    {
        private string message;
        private bool isError;
        private object data;
        
        public string Message
        {
            get => message;
            set => message = value;
        }

        public bool IsError
        {
            get => isError;
            set => isError = value;
        }

        public Messenger(string message, bool isError)
        {
            this.message = message;
            this.isError = isError;
        }

        public T GetData<T>()
        {
            return (T) data;
        }

        public void SetData<T>(T data)
        {
            this.data = data;
        }
        
    }
}