using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GsnMotionEasy.Model
{
    public class StatusInfo
    {
        public bool PlusLimitAlarm { get; internal set; }

        public bool MinusLimitAlarm { get; internal set; }

        public bool AxisAlarm { get; internal set; }

        public bool FollowAlarm { get; internal set; }

        public bool SmoothStopAlarm { get; internal set; }

        public bool Scram { get; internal set; }

        public bool EnableAxis { get; internal set; }

        public bool Planning { get; internal set; }

        public string MotionType { get; internal set; }

        public double PlannedLocation { get; internal set; }

        public double PlannedVel { get; internal set; }

        public double PlannedAccVel { get; internal set; }

        public double DriveLocation { get; internal set; }

        public double DriveVel { get; internal set; }

        public double DriveAccVel { get; internal set; }

        public double FollowErr { get; internal set; }
    }

}
