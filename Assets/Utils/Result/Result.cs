namespace Utils.Result
{
    public struct Result<T>
    {
        public readonly T Object;
        public readonly bool IsExist;

        public Result(T result, bool isExist)
        {
            Object = result;
            IsExist = isExist;
        }
        
        public static Result<T> Success(T result)
        {
            return new Result<T>(result, true);
        }

        public static Result<T> Fail()
        {
            return new Result<T>(default, false);
        }

        public bool Equals(Result<T> other)
        {
            return this.IsExist == other.IsExist &&
                   (IsExist == false || this.Object.Equals(other.Object));
        }
        
        public override bool Equals(object other)
        {
            if (!(other is Result<T>))
            {
                return false;
            }
            
            return Equals((Result<T>) other);
        }

        public override string ToString()
        {
            return !IsExist ? "Empty result" : Object.ToString();
        }

        public override int GetHashCode()
        {
            return !IsExist ? 0 : Object.GetHashCode();
        }
    }
}