﻿using System;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class SteamServerInfoPingDescendingComparator : IComparer<SteamServerInfo>
	{
		public int Compare(SteamServerInfo a, SteamServerInfo b)
		{
			return b.ping - a.ping;
		}
	}
}
