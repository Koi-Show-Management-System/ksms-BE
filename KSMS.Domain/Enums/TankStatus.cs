using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Enums
{
    public enum TankStatus
    {
        Available,      // Sẵn sàng
        Occupied,       // Đã có cá
        Maintenance,    // Đang bảo trì
        Cleaning,       // Đang vệ sinh
        Damaged,        // Hỏng 
        OutOfService    // Không sử dụng
    }

}
