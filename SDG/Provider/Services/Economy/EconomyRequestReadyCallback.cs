﻿using System;

namespace SDG.Provider.Services.Economy
{
	public delegate void EconomyRequestReadyCallback(IEconomyRequestHandle economyRequestHandle, IEconomyRequestResult economyRequestResult);
}
