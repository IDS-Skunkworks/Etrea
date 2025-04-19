using Newtonsoft.Json;
using System;
using System.Linq;
using Etrea3.Core;

namespace Etrea3.Objects
{
    [Serializable]
    public class Emote
    {
        [JsonProperty]
        public int ID { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string[] MessageToPerformer { get; set; } = { "", "", "", "" }; // 0 = no target, 1 = with target, 2 = target not found, 3 = target == performer
        [JsonProperty]
        public string MessageToTarget { get; set; }
        [JsonProperty]
        public string[] MessageToOthers { get; set; } = { "", "", "", "" }; // 0 = no target, 1 = with target, 2 = target not found, 3 = target == performer
        [JsonIgnore]
        public bool OLCLocked { get; set; }
        [JsonIgnore]
        public Guid LockHolder { get; set; } = Guid.Empty;

        public override string ToString()
        {
            return $"{ID}: {Name}";
        }

        public void Perform(Actor performer, Actor target, bool targetProvided, string tName)
        {
            string targetName = target != null ? target.Name : tName;
            string pMessage, tMessage = string.Empty;
            if (performer == null)
            {
                Game.LogMessage($"DEBUG: Emote '{Name}' (ID: {ID}) somehow called with a null performer", LogLevel.Debug);
                return;
            }
            if (target == null && targetProvided)
            {
                // we looked for a target but couldn't find one so send MessageToPerformer[2], MessageToOthers[2]
                if (performer.ActorType == ActorType.Player)
                {
                    pMessage = ParseMessageForPerformer(performer, target, MessageToPerformer[2], targetName);
                    ((Player)performer).Send($"{pMessage}{Constants.NewLine}");
                }
                SendToOthers(performer, target, MessageToOthers[2]);
                return;
            }
            if (target == null && !targetProvided)
            {
                // we didn't look for a target (none specified) so send MessageToPerformer[0], MessageToOthers[0]
                if (performer.ActorType == ActorType.Player)
                {
                    pMessage = ParseMessageForPerformer(performer, target, MessageToPerformer[0], targetName);
                    ((Player)performer).Send($"{pMessage}{Constants.NewLine}");
                }
                SendToOthers(performer, target, MessageToOthers[0]);
                return;
            }
            if (target == performer)
            {
                // emote against self, send MessageToPerformer[3] and MessageToOthers[3]
                if (performer.ActorType == ActorType.Player)
                {
                    pMessage = ParseMessageForPerformer(performer, target, MessageToPerformer[3], targetName);
                    ((Player)performer).Send($"{pMessage}{Constants.NewLine}");
                }
                SendToOthers(performer, target, MessageToOthers[3]);
                return;
            }
            // with a target, send MessageToPerformer[1], MessageToTarget and MessageToOthers[1]
            pMessage = ParseMessageForPerformer(performer, target, MessageToPerformer[1], targetName);
            if (performer.ActorType == ActorType.Player)
            {
                ((Player)performer).Send($"{pMessage}{Constants.NewLine}");
            }
            tMessage = ParseMessageForTarget(performer, target, MessageToTarget);
            if (target.ActorType == ActorType.Player)
            {
                ((Player)target).Send($"{tMessage}{Constants.NewLine}");
            }
            SendToOthers(performer, target, MessageToOthers[1]);
            if (target.ActorType == ActorType.NonPlayer && performer.ActorType == ActorType.Player)
            {
                NPC tgt = (NPC)target;
                Player perf = (Player)performer;
                if (tgt.MobProgs.Count > 0)
                {
                    foreach(var mp in tgt.MobProgs.Keys)
                    {
                        var mobProg = ScriptObjectManager.Instance.GetMobProg(mp);
                        if (mobProg != null)
                        {
                            mobProg.Init();
                            mobProg.TriggerEvent(MobProgTrigger.EmoteTarget, new { mob = tgt.ID.ToString(), player = perf.ID.ToString(), emotename = Name });
                        }
                    }
                }
            }
        }

        private string ParseMessageForPerformer(Actor performer, Actor target, string message, string targetName)
        {
            string parsedMessage = message;
            if (message.IndexOf("%pn%") >= 0)
            {
                parsedMessage = parsedMessage.Replace("%pn%", performer.Name);
            }
            if (message.IndexOf("%tn%") >= 0)
            {
                parsedMessage = target != null ? parsedMessage.Replace("%tn%", target.Name) : parsedMessage.Replace("%tn%", targetName);
            }
            if (message.IndexOf("%pg%") >= 0)
            {
                parsedMessage = parsedMessage.Replace("%pg%", performer.Gender.ToString());
            }
            if (message.IndexOf("%tg%") >= 0)
            {
                parsedMessage = target != null ? parsedMessage.Replace("%tg%", target.Gender.ToString()) : parsedMessage.Replace("%tg%", "it");
            }
            if (message.IndexOf("%pg1%") >= 0)
            {
                switch (performer.Gender)
                {
                    case Gender.Male:
                        parsedMessage = parsedMessage.Replace("%pg1%", Constants.ObjectivePronouns[0]);
                        break;

                    case Gender.Female:
                        parsedMessage = parsedMessage.Replace("%pg1%", Constants.ObjectivePronouns[1]);
                        break;

                    case Gender.NonBinary:
                        parsedMessage = parsedMessage.Replace("%pg1%", Constants.ObjectivePronouns[2]);
                        break;
                }
            }
            if (message.IndexOf("%pg2%") >= 0)
            {
                switch (performer.Gender)
                {
                    case Gender.Male:
                        parsedMessage = parsedMessage.Replace("%pg2%", Constants.PosessivePronouns[0]);
                        break;

                    case Gender.Female:
                        parsedMessage = parsedMessage.Replace("%pg2%", Constants.PosessivePronouns[1]);
                        break;

                    case Gender.NonBinary:
                        parsedMessage = parsedMessage.Replace("%pg2%", Constants.PosessivePronouns[2]);
                        break;
                }
            }
            if (message.IndexOf("%pg3%") >= 0)
            {
                switch (performer.Gender)
                {
                    case Gender.Male:
                        parsedMessage = parsedMessage.Replace("%pg3%", Constants.PersonalPronouns[0]);
                        break;

                    case Gender.Female:
                        parsedMessage = parsedMessage.Replace("%pg3%", Constants.PersonalPronouns[1]);
                        break;

                    case Gender.NonBinary:
                        parsedMessage = parsedMessage.Replace("%pg3%", Constants.PersonalPronouns[2]);
                        break;
                }
            }
            if (message.IndexOf("%tg1%") >= 0)
            {
                if (target == null)
                {
                    parsedMessage = parsedMessage.Replace("%tg1%", "it");
                }
                else
                {
                    switch (target.Gender)
                    {
                        case Gender.Male:
                            parsedMessage = parsedMessage.Replace("%tg1%", Constants.ObjectivePronouns[0]);
                            break;

                        case Gender.Female:
                            parsedMessage = parsedMessage.Replace("%tg1%", Constants.ObjectivePronouns[1]);
                            break;

                        case Gender.NonBinary:
                            parsedMessage = parsedMessage.Replace("%tg1%", Constants.ObjectivePronouns[2]);
                            break;

                        case Gender.Undefined:
                            parsedMessage = parsedMessage.Replace("%tg1%", "it");
                            break;
                    }
                }
            }
            if (message.IndexOf("%tg2%") >= 0)
            {
                if (target == null)
                {
                    parsedMessage = parsedMessage.Replace("%tg2%", "it");
                }
                else
                {
                    switch (target.Gender)
                    {
                        case Gender.Male:
                            parsedMessage = parsedMessage.Replace("%tg2%", Constants.PosessivePronouns[0]);
                            break;

                        case Gender.Female:
                            parsedMessage = parsedMessage.Replace("%tg2%", Constants.PosessivePronouns[1]);
                            break;

                        case Gender.NonBinary:
                            parsedMessage = parsedMessage.Replace("%tg2%", Constants.PosessivePronouns[2]);
                            break;

                        case Gender.Undefined:
                            parsedMessage = parsedMessage.Replace("%tg2%", "it");
                            break;
                    }
                }
            }
            if (message.IndexOf("%pg3%") >= 0)
            {
                if (target == null)
                {
                    parsedMessage = parsedMessage.Replace("%tg3%", "it");
                }
                else
                {
                    switch (target.Gender)
                    {
                        case Gender.Male:
                            parsedMessage = parsedMessage.Replace("%tg3%", Constants.PersonalPronouns[0]);
                            break;

                        case Gender.Female:
                            parsedMessage = parsedMessage.Replace("%tg3%", Constants.PersonalPronouns[1]);
                            break;

                        case Gender.NonBinary:
                            parsedMessage = parsedMessage.Replace("%tg3%", Constants.PersonalPronouns[2]);
                            break;

                        case Gender.Undefined:
                            parsedMessage = parsedMessage.Replace("%tg3%", "it");
                            break;
                    }
                }
            }
            return parsedMessage;
        }

        private string ParseMessageForTarget(Actor performer, Actor target, string message)
        {
            string parsedMessage = message;
            if (message.IndexOf("%pn%") >= 0)
            {
                parsedMessage = performer.CanBeSeenBy(target) ? parsedMessage.Replace("%pn%", performer.Name) : parsedMessage.Replace("%pn%", "Something");
            }
            if (message.IndexOf("%tn%") >= 0)
            {
                parsedMessage = parsedMessage.Replace("%tn%", target.Name);
            }
            if (message.IndexOf("%pg%") >= 0)
            {
                parsedMessage = parsedMessage.Replace("%pg%", performer.Gender.ToString());
            }
            if (message.IndexOf("%tg%") >= 0)
            {
                parsedMessage = parsedMessage.Replace("%tg%", target.Gender.ToString());
            }
            if (message.IndexOf("%pg1%") >= 0)
            {
                if (performer.CanBeSeenBy(target))
                {
                    switch (performer.Gender)
                    {
                        case Gender.Male:
                            parsedMessage = parsedMessage.Replace("%pg1%", Constants.ObjectivePronouns[0]);
                            break;

                        case Gender.Female:
                            parsedMessage = parsedMessage.Replace("%pg1%", Constants.ObjectivePronouns[1]);
                            break;

                        case Gender.NonBinary:
                            parsedMessage = parsedMessage.Replace("%pg1%", Constants.ObjectivePronouns[2]);
                            break;
                    }
                }
                else
                {
                    parsedMessage = parsedMessage.Replace("%pg1%", "it");
                }
            }
            if (message.IndexOf("%pg2%") >= 0)
            {
                if (performer.CanBeSeenBy(target))
                {
                    switch (performer.Gender)
                    {
                        case Gender.Male:
                            parsedMessage = parsedMessage.Replace("%pg2%", Constants.PosessivePronouns[0]);
                            break;

                        case Gender.Female:
                            parsedMessage = parsedMessage.Replace("%pg2%", Constants.PosessivePronouns[1]);
                            break;

                        case Gender.NonBinary:
                            parsedMessage = parsedMessage.Replace("%pg2%", Constants.PosessivePronouns[2]);
                            break;
                    }
                }
                else
                {
                    parsedMessage = parsedMessage.Replace("%pg2%", "its");
                }
                if (message.IndexOf("%pg3%") >= 0)
                {
                    if (performer.CanBeSeenBy(target))
                    {
                        switch (performer.Gender)
                        {
                            case Gender.Male:
                                parsedMessage = parsedMessage.Replace("%pg3%", Constants.PersonalPronouns[0]);
                                break;

                            case Gender.Female:
                                parsedMessage = parsedMessage.Replace("%pg3%", Constants.PersonalPronouns[1]);
                                break;

                            case Gender.NonBinary:
                                parsedMessage = parsedMessage.Replace("%pg3%", Constants.PersonalPronouns[2]);
                                break;
                        }
                    }
                    else
                    {
                        parsedMessage = parsedMessage.Replace("%pg3%", "it");
                    }
                }
                if (message.IndexOf("%tg1%") >= 0)
                {
                    switch (target.Gender)
                    {
                        case Gender.Male:
                            parsedMessage = parsedMessage.Replace("%tg1%", Constants.ObjectivePronouns[0]);
                            break;

                        case Gender.Female:
                            parsedMessage = parsedMessage.Replace("%tg1%", Constants.ObjectivePronouns[1]);
                            break;

                        case Gender.NonBinary:
                            parsedMessage = parsedMessage.Replace("%tg1%", Constants.ObjectivePronouns[2]);
                            break;
                    }
                }
                if (message.IndexOf("%tg2%") >= 0)
                {
                    switch (target.Gender)
                    {
                        case Gender.Male:
                            parsedMessage = parsedMessage.Replace("%tg2%", Constants.PosessivePronouns[0]);
                            break;

                        case Gender.Female:
                            parsedMessage = parsedMessage.Replace("%tg2%", Constants.PosessivePronouns[1]);
                            break;

                        case Gender.NonBinary:
                            parsedMessage = parsedMessage.Replace("%tg2%", Constants.PosessivePronouns[2]);
                            break;
                    }
                }
                if (message.IndexOf("%tg3%") >= 0)
                {
                    switch (target.Gender)
                    {
                        case Gender.Male:
                            parsedMessage = parsedMessage.Replace("%tg3%", Constants.PersonalPronouns[0]);
                            break;

                        case Gender.Female:
                            parsedMessage = parsedMessage.Replace("%tg3%", Constants.PersonalPronouns[1]);
                            break;

                        case Gender.NonBinary:
                            parsedMessage = parsedMessage.Replace("%tg3%", Constants.PersonalPronouns[2]);
                            break;
                    }
                }
            }
            return parsedMessage;
        }

        private string ParseMessageForOthers(Actor performer, Actor target, Actor messageTo, string message)
        {
            var parsedMessage = message;
            if (message.IndexOf("%pn%") >= 0)
            {
                parsedMessage = performer.CanBeSeenBy(messageTo) ? parsedMessage.Replace("%pn%", performer.Name) : "Someone";
            }
            if (message.IndexOf("%tn%") >= 0)
            {
                parsedMessage = target.CanBeSeenBy(messageTo) ? parsedMessage.Replace("%tn%", target.Name) : "Someone";
            }
            if (message.IndexOf("%pg1%") >= 0)
            {
                if (performer.CanBeSeenBy(messageTo))
                {
                    switch(performer.Gender)
                    {
                        case Gender.Male:
                            parsedMessage = parsedMessage.Replace("%pg1%", Constants.ObjectivePronouns[0]);
                            break;

                        case Gender.Female:
                            parsedMessage = parsedMessage.Replace("%pg1%", Constants.ObjectivePronouns[1]);
                            break;

                        case Gender.NonBinary:
                            parsedMessage = parsedMessage.Replace("%pg1%", Constants.ObjectivePronouns[2]);
                            break;
                    }
                }
                else
                {
                    parsedMessage = parsedMessage.Replace("%pg1%", "it");
                }
            }
            if (message.IndexOf("%pg2%") >= 0)
            {
                if (performer.CanBeSeenBy(messageTo))
                {
                    switch(performer.Gender)
                    {
                        case Gender.Male:
                            parsedMessage = parsedMessage.Replace("%pg2%", Constants.PosessivePronouns[0]);
                            break;

                        case Gender.Female:
                            parsedMessage = parsedMessage.Replace("%pg2%", Constants.PosessivePronouns[1]);
                            break;

                        case Gender.NonBinary:
                            parsedMessage = parsedMessage.Replace("%pg2%", Constants.PosessivePronouns[2]);
                            break;
                    }
                }
                else
                {
                    parsedMessage = parsedMessage.Replace("%pg2%", "its");
                }
            }
            if (message.IndexOf("%pg3%") >= 0)
            {
                if (performer.CanBeSeenBy(messageTo))
                {
                    switch (performer.Gender)
                    {
                        case Gender.Male:
                            parsedMessage = parsedMessage.Replace("%pg3%", Constants.PersonalPronouns[0]);
                            break;

                        case Gender.Female:
                            parsedMessage = parsedMessage.Replace("%pg3%", Constants.PersonalPronouns[1]);
                            break;

                        case Gender.NonBinary:
                            parsedMessage = parsedMessage.Replace("%pg3%", Constants.PersonalPronouns[2]);
                            break;
                    }
                }
                else
                {
                    parsedMessage = parsedMessage.Replace("%pg3%", "it");
                }
            }
            if (message.IndexOf("%tg1%") >= 0)
            {
                if (target.CanBeSeenBy(messageTo))
                {
                    switch (target.Gender)
                    {
                        case Gender.Male:
                            parsedMessage = parsedMessage.Replace("%tg1%", Constants.ObjectivePronouns[0]);
                            break;

                        case Gender.Female:
                            parsedMessage = parsedMessage.Replace("%tg1%", Constants.ObjectivePronouns[1]);
                            break;

                        case Gender.NonBinary:
                            parsedMessage = parsedMessage.Replace("%tg1%", Constants.ObjectivePronouns[2]);
                            break;
                    }
                }
                else
                {
                    parsedMessage = parsedMessage.Replace("%tg1%", "it");
                }
            }
            if (message.IndexOf("%tg2%") >= 0)
            {
                if (target.CanBeSeenBy(messageTo))
                {
                    switch (target.Gender)
                    {
                        case Gender.Male:
                            parsedMessage = parsedMessage.Replace("%tg2%", Constants.PosessivePronouns[0]);
                            break;

                        case Gender.Female:
                            parsedMessage = parsedMessage.Replace("%tg2%", Constants.PosessivePronouns[1]);
                            break;

                        case Gender.NonBinary:
                            parsedMessage = parsedMessage.Replace("%tg2%", Constants.PosessivePronouns[2]);
                            break;
                    }
                }
                else
                {
                    parsedMessage = parsedMessage.Replace("%tg2%", "its");
                }
            }
            if (message.IndexOf("%tg3%") >= 0)
            {
                if (target.CanBeSeenBy(messageTo))
                {
                    switch (target.Gender)
                    {
                        case Gender.Male:
                            parsedMessage = parsedMessage.Replace("%tg3%", Constants.PersonalPronouns[0]);
                            break;

                        case Gender.Female:
                            parsedMessage = parsedMessage.Replace("%tg3%", Constants.PersonalPronouns[1]);
                            break;

                        case Gender.NonBinary:
                            parsedMessage = parsedMessage.Replace("%tg3%", Constants.PersonalPronouns[2]);
                            break;
                    }
                }
                else
                {
                    parsedMessage = parsedMessage.Replace("%tg3%", "it");
                }
            }
            return parsedMessage;
        }

        private void SendToOthers(Actor performer, Actor target, string message)
        {
            var localPlayers = SessionManager.Instance.GetPlayersInRoom(performer.CurrentRoom).Where(x => x.ID != performer.ID && x.ID != target?.ID).ToList();
            if (localPlayers != null && localPlayers.Count > 0)
            {
                foreach(var player in localPlayers)
                {
                    var msg = ParseMessageForOthers(performer, target, player.Player, message);
                    player.Send($"{msg}{Constants.NewLine}");
                }
            }
        }
    }
}
