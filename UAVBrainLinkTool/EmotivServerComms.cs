using Emotiv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Wrapper and class for holding methods to obtain a user profile for interpreting commands
// Much of this code is inspired by the Emotiv examples found here:
// https://github.com/Emotiv/community-sdk/tree/master/examples_basic/C%23

namespace UAVBrainLinkTool
{
    public static class EmotivServerComms
    {
        static int userCloudID = 0;

        public static Boolean initialize()
        {
            return true;
        }

        public static Boolean logIn(String userName, String password)
        {
            if (EmotivCloudClient.EC_Connect() != EdkDll.EDK_OK)
            {
                Logging.outputLine("Cannot connect to Emotiv Cloud!");
                return false;
            }

            if (EmotivCloudClient.EC_Login(userName, password) != EdkDll.EDK_OK)
            {
                Logging.outputLine("Login attempt failed! Username and/or password may be incorrect.");
                return false;
            }

            Logging.outputLine("Logged in as \"" + userName + "\".");

            if (EmotivCloudClient.EC_GetUserDetail(ref userCloudID) != EdkDll.EDK_OK)
            {
                Logging.outputLine("Failed to get user!");
                return false;
            }

            return true;
        }

        public static Boolean loadUserProfile(String profileName)
        {
            int profileID = -1;
            EmotivCloudClient.EC_GetProfileId(userCloudID, profileName, ref profileID);

            if (EmotivCloudClient.EC_LoadUserProfile(userCloudID, 0, profileID, Constants.version) == EdkDll.EDK_OK)
            {
                Logging.outputLine("Profile loading completed.");
                return true;
            }
            else
            {
                Logging.outputLine("Profile loading failed!");
                return false;
            }
        }
    }
}
