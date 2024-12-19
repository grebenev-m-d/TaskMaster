CREATE DATABASE TaskMaster;

USE TaskMaster;

CREATE TABLE [file] (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- уникальный идентификатор
    [relativePath] NVARCHAR(MAX) NOT NULL, -- путь
	[fileName] NVARCHAR(255) NOT NULL, -- путь
	createdAt DATETIME2 DEFAULT GETDATE(), -- дата создания
);

CREATE TABLE [role] (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- уникальный идентификатор роли
    [type] NVARCHAR(50) NOT NULL, -- название роли
);

CREATE TABLE [user] (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- уникальный идентификатор пользователя
    [name] NVARCHAR(255) NOT NULL, -- имя пользователя
    passwordHash NVARCHAR(255) NOT NULL, -- хэш пароля
    email NVARCHAR(255) UNIQUE NOT NULL, -- электронная почта
    createdAt DATETIME2 DEFAULT GETDATE(), -- дата создания учетной записи
    roleId UNIQUEIDENTIFIER, -- идентификатор роли
    FOREIGN KEY (roleId) REFERENCES role(id)
);

CREATE TABLE [accessLevel] (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- уникальный идентификатор уровня доступа
    [type] VARCHAR(50)  NOT NULL,-- тип разрешения
);

CREATE TABLE [design] (
	id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- уникальный идентификатор 
    [type] VARCHAR(50) NOT NULL,-- тип дизайна
)

CREATE TABLE [board] (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- уникальный идентификатор доски
    title NVARCHAR(255), -- название доски
	createdAt DATETIME2 DEFAULT GETDATE(),
	userId UNIQUEIDENTIFIER, -- идентификатор создателя

	designTypeId UNIQUEIDENTIFIER, -- тип дизайна: цвет, изображение, градиент и т.д.
    colorCode VARCHAR(10), -- код цвета доски
    imageFileId UNIQUEIDENTIFIER, -- изображение доски
    
	isPublic BIT NOT NULL DEFAULT 0, -- По умолчанию доска приватная
    generalAccessLevelId UNIQUEIDENTIFIER, --разрешение для всех

    FOREIGN KEY (generalAccessLevelId) REFERENCES [accessLevel](id),
    FOREIGN KEY (userId) REFERENCES [user](id),
	FOREIGN KEY (designTypeId) REFERENCES design(id),
	FOREIGN KEY ( imageFileId) REFERENCES [file](id)
);


CREATE TABLE [boardAccessLevelMap] (
    boardId UNIQUEIDENTIFIER,
    userId UNIQUEIDENTIFIER,
    accessLevelId UNIQUEIDENTIFIER,
    FOREIGN KEY (boardId) REFERENCES board(id),
    FOREIGN KEY (userId) REFERENCES [user](id),
    FOREIGN KEY (accessLevelId) REFERENCES accessLevel(id),
    PRIMARY KEY (boardId, userId) -- уникальный ключ состоящий из boardId и userId
);
CREATE TABLE boardViewHistoryMap (
    userId UNIQUEIDENTIFIER NOT NULL,
    boardId UNIQUEIDENTIFIER NOT NULL,
    lastViewedAt DATETIME2 DEFAULT GETDATE(),
    PRIMARY KEY (userId, boardId),
    FOREIGN KEY (userId) REFERENCES [user](id),
	FOREIGN KEY (boardId) REFERENCES [board](id)
);

CREATE TABLE [cardList] (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- уникальный идентификатор колонки
    title NVARCHAR(255) NOT NULL, -- название колонки

	boardId UNIQUEIDENTIFIER, -- идентификатор доски
    prevCardListId UNIQUEIDENTIFIER, -- идентификатор предыдущей колонки
    nextCardListId UNIQUEIDENTIFIER, -- идентификатор следующей колонки
    FOREIGN KEY (boardId) REFERENCES board(id),
    FOREIGN KEY (prevCardListId) REFERENCES [cardList](id),
    FOREIGN KEY (nextCardListId) REFERENCES [cardList](id)
);

CREATE TABLE [card] (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- уникальный идентификатор карточки
    title NVARCHAR(255) NOT NULL, -- заголовок карточки
    [description] NVARCHAR(MAX), -- описание карточки

	imageFileId UNIQUEIDENTIFIER, -- идентификатор колонки
    cardListId UNIQUEIDENTIFIER, -- идентификатор колонки
    prevCardId UNIQUEIDENTIFIER, -- идентификатор предыдущей карточки
    nextCardId UNIQUEIDENTIFIER, -- идентификатор следующей карточки

	FOREIGN KEY (imageFileId) REFERENCES [file](id),
    FOREIGN KEY (cardListId) REFERENCES [cardList](id),
    FOREIGN KEY (prevCardId) REFERENCES [card](id),
    FOREIGN KEY (nextCardId) REFERENCES [card](id)
);

CREATE TABLE [cardAttachment](
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- уникальный идентификатор ссылки
    fileId UNIQUEIDENTIFIER NOT NULL,
    cardId UNIQUEIDENTIFIER, -- идентификатор карточки, к которой относится ссылка
	createdAt DATETIME2 DEFAULT GETDATE(), -- дата создания
    FOREIGN KEY (cardId) REFERENCES [card](id),
	FOREIGN KEY (fileId) REFERENCES [file](id)
);

CREATE TABLE [cardComment] (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- уникальный идентификатор комментария
    [text] NVARCHAR(MAX) NOT NULL, -- текст комментария
	createdAt DATETIME2 DEFAULT GETDATE(), -- дата создания
	updatedAt DATETIME2 DEFAULT GETDATE(), -- дата обновления	
    cardId UNIQUEIDENTIFIER, -- идентификатор карточки, к которой относится комментарий
	userId UNIQUEIDENTIFIER, -- идентификатор автора комментария
    FOREIGN KEY (cardId) REFERENCES [card](id),
	FOREIGN KEY (userId) REFERENCES [user](id) -- Связь с 
);


INSERT INTO accessLevel (type) VALUES 
('reader'),('editor'),('owner');

INSERT INTO role (type) VALUES 
('user'),('admin');

INSERT INTO design (type) VALUES 
('color'),('image');