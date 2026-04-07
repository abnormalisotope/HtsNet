namespace HtsNet
{
    public static class HtsConst
    {
        //GLOBAL
        public const string LABEL_GLOBAL = "[GLOBAL]";
        public const string HTS_VOICE_VERSION = "HTS_VOICE_VERSION";
        public const string SAMPLING_FREQUENCY = "SAMPLING_FREQUENCY";
        public const string FRAME_PERIOD = "FRAME_PERIOD";
        public const string NUM_STATES = "NUM_STATES";
        public const string NUM_STREAMS = "NUM_STREAMS";
        public const string STREAM_TYPE = "STREAM_TYPE";
        public const string FULLCONTEXT_FORMAT = "FULLCONTEXT_FORMAT";
        public const string FULLCONTEXT_VERSION = "FULLCONTEXT_VERSION";
        public const string GV_OFF_CONTEXT = "GV_OFF_CONTEXT";
        public const string COMMENT = "COMMENT";
        //STREAM
        public const string LABEL_STREAM = "[STREAM]";
        public const string VECTOR_LENGTH = "VECTOR_LENGTH";
        public const string IS_MSD = "IS_MSD";
        public const string NUM_WINDOWS = "NUM_WINDOWS";
        public const string USE_GV = "USE_GV";
        public const string USE_AVAILABLE_RANGE = "USE_AVAILABLE_RANGE";
        public const string OPTION = "OPTION";
        public const string USE_VOICE_MODIFICATION = "USE_VOICE_MODIFICATION";
        public const string SLUR_SMOOTHING_PATTERNS = "SLUR_SMOOTHING_PATTERNS";
        public const string SMOOTHING_TYPE = "SMOOTHING_TYPE";

        //POSITION
        public const string LABEL_POSITION = "[POSITION]";
        public const string DURATION_PDF = "DURATION_PDF";
        public const string DURATION_TREE = "DURATION_TREE";
        public const string PDURATION_PDF = "PDURATION_PDF";
        public const string PDURATION_TREE = "PDURATION_TREE";
        public const string REALIGNMENT_C_PDF = "REALIGNMENT_C_PDF";
        public const string REALIGNMENT_C_TREE = "REALIGNMENT_C_TREE";
        public const string REALIGNMENT_S_PDF = "REALIGNMENT_S_PDF";
        public const string REALIGNMENT_S_TREE = "REALIGNMENT_S_TREE";
        public const string STREAM_WIN = "STREAM_WIN";
        public const string STREAM_PDF = "STREAM_PDF";
        public const string STREAM_TREE = "STREAM_TREE";
        public const string GV_PDF = "GV_PDF";
        public const string GV_TREE = "GV_TREE";
        public const string AVAILABLE_RANGE_PDF = "AVAILABLE_RANGE_PDF";

        //DATA
        public const string LABEL_DATA = "[DATA]";

        //DEFAULT
        public const string DEFAULT_GV_OFF_CONTEXT = "\"*-sil+*\",\"*-pau+*\"";
        public const string DEFAULT_FULLCONTEXT_FORMAT_TALK = "HTS_TTS_JPN";
        public const string DEFAULT_FULLCONTEXT_FORMAT_SONG = "HTS_SVSS_JPN";
    }
}
