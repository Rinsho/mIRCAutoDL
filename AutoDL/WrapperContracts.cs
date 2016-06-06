/*
 * TO-DO: Possibly rename function? Different interface but the
 * name is the same as the IDownloadCallback function and has a
 * similar function just on a different layer.  Hmm.
 */


using System;

namespace AutoDL.WrapperContracts
{
    /* Interface: IUpdate
     * Desription: Defines the contract for sending messages with wrapper.
     */
    public interface IUpdate
    {
        //DownloadUpdate: Receives success message from wrapper
        //                and handles sending new download and 
        //                updating the UI.
        void DownloadStatusUpdate(bool success);
    }
}
