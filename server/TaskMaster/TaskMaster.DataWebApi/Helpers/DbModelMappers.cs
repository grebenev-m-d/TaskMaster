using System.Data;
using System.Security;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataWebApi.Models;

namespace TaskMaster.DataWebApi.Helpers
{
	/// <summary>
	/// Класс, отвечающий за отображение моделей базы данных в модели приложения и наоборот.
	/// </summary>
	public static class DbModelMappers
	{
		#region Board
		/// <summary>
		/// Преобразует список DbBoard в список Board.
		/// </summary>
		/// <param name="dbBoards">Список DbBoard.</param>
		/// <returns>Список Board.</returns>
		public static List<Board> MapDbToBoard(List<DbBoard> dbBoards)
		{
			if (dbBoards == null) return null;

			var boards = new List<Board>();

			for (var i = 0; i < dbBoards.Count; i++)
			{
				boards.Add(MapDbToBoard(dbBoards[i]));
			}

			return boards;
		}

		/// <summary>
		/// Преобразует DbBoard в Board.
		/// </summary>
		/// <param name="dbBoard">DbBoard.</param>
		/// <returns>Board.</returns>
		public static Board MapDbToBoard(DbBoard dbBoard)
		{
			if (dbBoard == null) return null;

			var board = new Board()
			{
				Id = dbBoard.Id,
				Title = dbBoard.Title,
				CreatedAt = dbBoard.CreatedAt,
				ColorCode = dbBoard.ColorCode,
			};

			if (dbBoard.DesignType != null)
			{
				board.DesignType = dbBoard.DesignType.Type;
			}

			if (dbBoard.User != null)
			{
				board.Owner = MapDbToUser(dbBoard.User);
			}

			return board;
		}
		#endregion

		/// <summary>
		/// Преобразует DbBoard в BoardAccessLevel.
		/// </summary>
		/// <param name="dbBoard">DbBoard.</param>
		/// <returns>BoardAccessLevel.</returns>
		public static BoardAccessLevel MapDbBoardToBoardAccessLevel(DbBoard dbBoard)
		{
			if (dbBoard == null) return null;

			var boardAccessLevel = new BoardAccessLevel()
			{
				IsPublic = dbBoard.IsPublic,
			};
			if (dbBoard.GeneralAccessLevel != null)
			{
				boardAccessLevel.DefaultAccessLevelType = dbBoard.GeneralAccessLevel.Type;
			}
			if (dbBoard.BoardAccessLevelMaps != null)
			{
				var maps = dbBoard.BoardAccessLevelMaps.ToList();
				boardAccessLevel.PersonalAccessLevels = new List<UserAccessLevel>();

				for (var i = 0; i < dbBoard.BoardAccessLevelMaps.Count; i++)
				{
					var map = new UserAccessLevel();

					if (maps[i].AccessLevel != null)
					{
						map.User = MapDbToUser(maps[i].User);
						map.AccessLevel = maps[i].AccessLevel.Type;
					}

					boardAccessLevel.PersonalAccessLevels.Add(map);
				}
			}

			return boardAccessLevel;
		}

		#region CardList
		/// <summary>
		/// Преобразует список CardList в список DbCardList.
		/// </summary>
		/// <param name="cardList">Список CardList.</param>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Список DbCardList.</returns>
		public static List<DbCardList> MapCardListToDb(List<CardList> cardList, Guid boardId)
		{
			if (cardList == null) return null;

			var dbCardList = new List<DbCardList>();

			for (var i = 0; i < cardList.Count; i++)
			{
				Guid? prevCardListId = i > 0 ? cardList[i - 1].Id : null;
				Guid? nextCardListId = i < cardList.Count - 1 ? cardList[i + 1].Id : null;

				dbCardList.Add(MapCardListToDb(cardList[i], prevCardListId, nextCardListId, boardId));
			}

			return dbCardList;
		}


		/// <summary>
		/// Преобразует CardList в DbCardList.
		/// </summary>
		/// <param name="cardList">CardList.</param>
		/// <param name="prevCardListId">Идентификатор предыдущего списка карточек.</param>
		/// <param name="nextCardListId">Идентификатор следующего списка карточек.</param>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>DbCardList.</returns>
		public static DbCardList MapCardListToDb(CardList cardList, Guid? prevCardListId, Guid? nextCardListId, Guid boardId)
		{
			if (cardList == null) return null;

			return new DbCardList()
			{
				Id = cardList.Id,
				Title = cardList.Title,
				PrevCardListId = prevCardListId,
				NextCardListId = nextCardListId,
				BoardId = boardId
			};
		}

		/// <summary>
		/// Преобразует список DbCardList в список CardList.
		/// </summary>
		/// <param name="dbCardList">Список DbCardList.</param>
		/// <returns>Список CardList.</returns>
		public static List<CardList> MapDbToCardList(List<DbCardList> dbCardList)
		{
			if (dbCardList == null) return null;

			var cardList = new List<CardList>();

			for (var i = 0; i < dbCardList.Count; i++)
			{
				cardList.Add(MapDbToCardList(dbCardList[i]));
			}

			return cardList;
		}

		/// <summary>
		/// Преобразует DbCardList в CardList.
		/// </summary>
		/// <param name="cardList">DbCardList.</param>
		/// <returns>CardList.</returns>
		public static CardList MapDbToCardList(DbCardList cardList)
		{
			if (cardList == null) return null;

			return new CardList()
			{
				Id = cardList.Id,
				Title = cardList.Title,
			};
		}
		#endregion

		#region Card
		/// <summary>
		/// Преобразует список DbCard в список Card.
		/// </summary>
		/// <param name="dbCards">Список DbCard.</param>
		/// <returns>Список Card.</returns>
		public static List<Card> MapDbToCard(List<DbCard> dbCards)
		{
			if (dbCards == null) return null;

			var cards = new List<Card>();

			for (var i = 0; i < dbCards.Count; i++)
			{
				cards.Add(MapDbToCard(dbCards[i]));
			}

			return cards;
		}

		/// <summary>
		/// Преобразует DbCard в Card.
		/// </summary>
		/// <param name="dbCard">DbCard.</param>
		/// <returns>Card.</returns>
		public static Card MapDbToCard(DbCard dbCard)
		{
			if (dbCard == null) return null;

			return new Card()
			{
				Id = dbCard.Id,
				Title = dbCard.Title,
				Description = dbCard.Description,
				HasCoverImage = dbCard.ImageFileId != null,
				Attachments = dbCard.CardAttachments
					.Select(a => new CardAttachment()
					{
						Id = a.Id,
						FileName = a.File.FileName
					})
					.ToList(),
			};
		}
		#endregion

		#region Comment
		/// <summary>
		/// Преобразует список DbCardComment в список CardComment.
		/// </summary>
		/// <param name="dbComments">Список DbCardComment.</param>
		/// <returns>Список CardComment.</returns>
		public static List<CardComment> MapDbToComment(List<DbCardComment> dbComments)
		{
			if (dbComments == null) return null;

			var comments = new List<CardComment>();

			for (var i = 0; i < dbComments.Count; i++)
			{
				comments.Add(MapDbToComment(dbComments[i]));
			}

			return comments;
		}

		/// <summary>
		/// Преобразует DbCardComment в CardComment.
		/// </summary>
		/// <param name="dbComment">DbCardComment.</param>
		/// <returns>CardComment.</returns>
		public static CardComment MapDbToComment(DbCardComment dbComment)
		{
			if (dbComment == null) return null;

			var cardComment = new CardComment()
			{
				Id = dbComment.Id,
				Text = dbComment.Text,
				CreatedAt = dbComment.CreatedAt,
				UpdatedAt = dbComment.UpdatedAt,
			};

			if (dbComment.User != null)
			{
				cardComment.User = MapDbToUser(dbComment.User);
			}
			return cardComment;
		}
		#endregion



		/// <summary>
		/// Преобразует список DbUser в список User.
		/// </summary>
		/// <param name="dbUsers">Список DbUser.</param>
		/// <returns>Список User.</returns>
		public static List<User> MapDbToUser(List<DbUser> dbUsers)
		{
			if (dbUsers == null) return null;

			var users = new List<User>();

			for (var i = 0; i < dbUsers.Count; i++)
			{
				users.Add(MapDbToUser(dbUsers[i]));
			}

			return users;
		}

		/// <summary>
		/// Преобразует DbUser в User.
		/// </summary>
		/// <param name="dbUser">DbUser.</param>
		/// <returns>User.</returns>
		public static User MapDbToUser(DbUser dbUser)
		{
			if (dbUser == null) return null;

			User user = new User()
			{
				Id = dbUser.Id
			};

			if (dbUser.Name != null)
			{
				user.Name = dbUser.Name;
			}

			if (dbUser.Email != null)
			{
				user.Email = dbUser.Email;
			}

			user.CreatedAt = dbUser.CreatedAt;

			if (dbUser.Role != null && dbUser.Role.Type != null)
			{
				user.Role = dbUser.Role.Type;
			}

			return user;
		}
		

		/// <summary>
		/// Преобразует DbCardAttachment в CardAttachment.
		/// </summary>
		/// <param name="dbCardAttachment">DbCardAttachment.</param>
		/// <returns>CardAttachment.</returns>
		public static CardAttachment MapDbToCardAttachment(DbCardAttachment dbCardAttachment)
		{
			var cardAttachment = new CardAttachment()
			{
				Id = dbCardAttachment.Id

			};
			if (dbCardAttachment.File != null)
			{
				cardAttachment.FileName = dbCardAttachment.File.FileName;
			}

			return cardAttachment;
		}
	}
}
