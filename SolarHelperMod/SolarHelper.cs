using System;
using System.Collections.Generic;
using Eleon.Modding;
using System.Diagnostics;


namespace SolarHelperMod
{
	public class SolarHelper : ModInterface
	{
		ModGameAPI	mGameAPI;

		const int	SolarBattery	=1495;

		
		public void Game_Start(ModGameAPI dediAPI)
		{
			mGameAPI	=dediAPI;
		}
		
		
		public void Game_Event(CmdId eventId, ushort seqNr, object data)
		{
			try
			{
				switch(eventId)
				{
					case	CmdId.Event_Playfield_Loaded:
					{
						mGameAPI.Console_Write("Event_Playfield_Loaded actually happens");
						break;
					}
					case	CmdId.Event_Playfield_List:
					{
						PlayfieldList	pfl	=data as PlayfieldList;
						foreach(string pf in pfl.playfields)
						{
							mGameAPI.Console_Write("Playfield: " + pf);
						}
					}
					break;

					case	CmdId.Event_GlobalStructure_List:
					{
						HandleGlobalStructureList(data as GlobalStructureList);
					}
					break;

					case	CmdId.Event_Structure_BlockStatistics:
					{
						IdStructureBlockInfo	idsbi	=(IdStructureBlockInfo)data;
						mGameAPI.Console_Write("Got block stats for id: " + idsbi.id);
						foreach(KeyValuePair<int, int> stat in idsbi.blockStatistics)
						{
							if(stat.Key == SolarBattery)
							{
								mGameAPI.Console_Write("Building has " + stat.Value + " solar batteries...");
							}
						}
					}
					break;

					case	CmdId.Event_ChatMessage:
					{
						ChatInfo	ci	=(ChatInfo)data;
						if(ci == null)
						{
							break;
						}

						if(ci.type != 8 && ci.type != 7 && ci.msg == "!MODS")
						{
							PrivateMessage(ci.playerId, "Solar Helper by Kharzette");
						}
					}
					break;
					
					case CmdId.Event_Playfield_Entity_List:
					{
						PlayfieldEntityList	pfel	=(PlayfieldEntityList)data;
						if(pfel == null)
						{
							break;
						}

						mGameAPI.Console_Write("Entity list for playfield " + pfel.playfield);
						foreach(EntityInfo ei in pfel.entities)
						{
							mGameAPI.Console_Write("ID: " + ei.id + ", Pos: " + ei.pos + ", Type: " + ei.type);
						}
					}
					break;
					
					default:
					break;
				}
			}
			catch(Exception ex)
			{
				mGameAPI.Console_Write(ex.Message);
			}
		}
		
		
		public void Game_Update()
		{
		}
		
		
		public void Game_Exit()
		{
			if(mGameAPI != null)
			{
				mGameAPI.Console_Write("SolarHelper: OldAPIExit");
			}
		}


		void HandleGlobalStructureList(GlobalStructureList gsl)
		{
			if(gsl == null)
			{
				return;
			}

			foreach(KeyValuePair<string, List<GlobalStructureInfo>> pfstructs in gsl.globalStructures)
			{
				mGameAPI.Console_Write("GSL for " + pfstructs.Key);
				foreach(GlobalStructureInfo gsi in pfstructs.Value)
				{
					if(gsi.coreType == 1)	//player core
					{
						mGameAPI.Console_Write("Player Structure: " + gsi.name + ", ID: " + gsi.id);
						mGameAPI.Console_Write("Powered: " + gsi.powered);
						mGameAPI.Console_Write("Fuel Remaining: " + gsi.fuel);						

						mGameAPI.Game_Request(CmdId.Request_Structure_BlockStatistics, (ushort)0, new Id(gsi.id));
					}
				}
			}
		}


		#region Chat Stuff
		void PrivateMessage(int entID, string msg)
		{
			string	cmd	="say p:" + entID + " '" + msg + "'";
			mGameAPI.Game_Request(CmdId.Request_ConsoleCommand, 0, new Eleon.Modding.PString(cmd));
		}
		
		
		void ChatMessage(string msg)
		{
			string	command = "SAY '" + msg + "'";
			mGameAPI.Game_Request(CmdId.Request_ConsoleCommand, 0, new Eleon.Modding.PString(command));
		}
		
		
		void NormalMessage(string msg)
		{
			mGameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, 0, new IdMsgPrio(0, msg, 0, 100));
		}
		
		
		void AlertMessage(string msg)
		{
			mGameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, 0, new IdMsgPrio(0, msg, 1, 100));
		}
		
		
		void AttentionMessage(string msg)
		{
			mGameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(0, msg, 2, 100));
		}
#endregion
	}
}