using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace AutoDL.ServiceContracts
{
    /// <summary>
    /// Defines the contract for subscribing to download updates.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IReceiveUpdatesCallback))]
    public interface IReceiveUpdates
    {
        [OperationContract(IsOneWay = false, IsInitiating = true)]
        void Subscribe();

        [OperationContract(IsOneWay = false, IsTerminating = true)]
        void Unsubscribe();
    }

    /// <summary>
    /// Defines the callback contract for sending download updates.
    /// </summary>
    public interface IReceiveUpdatesCallback
    {
        //Updates for current download
        [OperationContract(IsOneWay = true)]
        void StatusUpdate(DownloadStatus status);

        //Updates for next download
        [OperationContract(IsOneWay = true)]
        void DownloadingNext(Download download);
    }
}
