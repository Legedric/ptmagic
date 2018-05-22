using System;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

namespace Core.Helper {
  public static class TelegramHelper {
    public static void SendMessage(string botToken, Int64 chatId, string message, bool useSilentMode, LogHelper log) {
      if (!botToken.Equals("") && chatId != 0) {
        try {
          TelegramBotClient botClient = new TelegramBotClient(botToken);
          System.Threading.Tasks.Task<Message> sentMessage = botClient.SendTextMessageAsync(chatId, message, ParseMode.Markdown, false, useSilentMode);

          if (sentMessage.IsCompleted) {
            log.DoLogDebug("Telegram message sent to ChatId " + chatId.ToString() + " on Bot Token '" + botToken + "'");
          }
        } catch (Exception ex) {
          log.DoLogCritical("Exception sending telegram message to ChatId " + chatId.ToString() + " on Bot Token '" + botToken + "'", ex);
        }
      }
    }
  }
}
