﻿using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using NAPS2.Scan;

namespace NAPS2.Remoting.ClientServer
{
    [ServiceContract(CallbackContract = typeof(IScanCallback))]
    public interface IScanService
    {
        [OperationContract]
        List<string> GetSupportedDriverNames();

        [OperationContract]
        List<ScanDevice> GetDeviceList(ScanProfile scanProfile);

        [OperationContract]
        Task<int> Scan(ScanProfile scanProfile, ScanParams scanParams);

        [OperationContract(IsOneWay = true)]
        void CancelScan();
    }
}
