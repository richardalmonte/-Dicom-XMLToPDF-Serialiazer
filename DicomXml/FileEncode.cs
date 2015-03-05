namespace DicomXml
{
    /// <summary>
    /// Using the TransferSyntaxUID tag (0002,0010) to identify the file encoding mode.
    /// </summary>
    public enum FileEncode
    {
        /// <summary>
        /// if using TransferSyntaxUID = 1.2.840.10008.1.2
        /// </summary>
        ImplicitVR,
        /// <summary>
        /// if using TransferSyntaxUID = 1.2.840.10008.1.2.1 
        /// </summary>
        ExplicitLittle,
        /// <summary>
        /// if using TransferSyntaxUID = 1.2.840.10008.1.2.2
        /// </summary>
        ExplicitBig,
        /// <summary>
        /// if using TransferSyntaxUID = 1.2.840.10008.1.2.4.91
        /// </summary>
        jpeg
    }
}