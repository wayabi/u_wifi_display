using System;

class MyDeq
{
    int size_;
    byte[] buf_;
    int idx_start_;
    int idx_end_;

    public MyDeq(int size)
    {
        size_ = size + 1;
        buf_ = new byte[size_];
        idx_start_ = 0;
        idx_end_ = 0;
    }

    public void Push(ref byte[] buf, int size)
    {
        int num = size;
        int idx_buf = 0;
        if (num <= 0) return;
        if (num > size_ - 1)
        {
            idx_buf += (num - (size_ - 1));
            num = size_ - 1;
        }
        int idst = idx_end_;
        int w0 = num;
        if (idst + num > size_ - 1)
        {
            w0 = size_ - idst;
            if (idx_start_ > idx_end_ || idx_start_ == 0)
            {
                idx_start_ = 1;
            }
            idx_end_ = 0;
        }
        else
        {
            if (idx_start_ > idx_end_)
            {
                idx_end_ += w0;
                if (idx_end_ >= idx_start_)
                {
                    idx_start_ = idx_end_ + 1;
                }
            }
            else
            {
                idx_end_ += w0;
            }
        }
        Array.Copy(buf, idx_buf, buf_, idst, w0);
        int w1 = num - w0;
        if (w1 == 0)
        {
            return;
        }
        idx_end_ = w1;
        if (idx_end_ >= idx_start_)
        {
            idx_start_ = idx_end_ + 1;
        }
        Array.Copy(buf, w0 + idx_buf, buf_, 0, w1);
    }

    public int GetArray(ref byte[] buf)
    {
        if (idx_start_ == idx_end_)
        {
            return 0;
        }
        if (idx_end_ > idx_start_)
        {
            int num = idx_end_ - idx_start_;
            Array.Copy(buf_, idx_start_, buf, 0, num);
            return num;
        }
        else
        {
            int w0 = size_ - idx_start_;
            Array.Copy(buf_, idx_start_, buf, 0, w0);
            int w1 = idx_end_;
            if (w1 == 0) return w0;
            Array.Copy(buf_, 0, buf, w0, w1);
            return w0 + w1;
        }
    }

    public int GetNumData()
    {
        if (idx_end_ >= idx_start_)
        {
            return idx_end_ - idx_start_;
        }
        else
        {
            return size_ - (idx_start_ - idx_end_);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="num">request num to pop</param>
    /// <param name="buf">popped data</param>
    /// <returns>actual num popped data</returns>
    public int Pop(int num, ref byte[] buf)
    {
        int num0 = GetNumData();
        if (num > num0) num = num0;
        if (num == 0) return 0;
        if (idx_end_ >= idx_start_)
        {
            Array.Copy(buf_, idx_start_, buf, 0, num);
            idx_start_ += num;
            if (idx_start_ > size_)
            {
                idx_start_ -= size_;
            }
            return num;
        }
        else
        {
            if (idx_start_ + num > size_)
            {
                int num1_0 = size_ - idx_start_;
                Array.Copy(buf_, idx_start_, buf, 0, num1_0);
                int num1_1 = num - num1_0;
                Array.Copy(buf_, 0, buf, num1_0, num1_1);
                idx_start_ = num1_1;
                return num1_0 + num1_1;
            }
            else
            {
                Array.Copy(buf_, idx_start_, buf, 0, num);
                idx_start_ += num;
                if (idx_start_ > size_)
                {
                    idx_start_ = 0;
                }
                return num;
            }
        }
    }

    public void Clear()
    {
        idx_end_ = idx_start_;
    }


};
