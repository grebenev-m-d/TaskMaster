CREATE DATABASE TaskMaster;

USE TaskMaster;

CREATE TABLE [file] (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- ���������� �������������
    [relativePath] NVARCHAR(MAX) NOT NULL, -- ����
	[fileName] NVARCHAR(255) NOT NULL, -- ����
	createdAt DATETIME2 DEFAULT GETDATE(), -- ���� ��������
);

CREATE TABLE [role] (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- ���������� ������������� ����
    [type] NVARCHAR(50) NOT NULL, -- �������� ����
);

CREATE TABLE [user] (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- ���������� ������������� ������������
    [name] NVARCHAR(255) NOT NULL, -- ��� ������������
    passwordHash NVARCHAR(255) NOT NULL, -- ��� ������
    email NVARCHAR(255) UNIQUE NOT NULL, -- ����������� �����
    createdAt DATETIME2 DEFAULT GETDATE(), -- ���� �������� ������� ������
    roleId UNIQUEIDENTIFIER, -- ������������� ����
    FOREIGN KEY (roleId) REFERENCES role(id)
);

CREATE TABLE [accessLevel] (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- ���������� ������������� ������ �������
    [type] VARCHAR(50)  NOT NULL,-- ��� ����������
);

CREATE TABLE [design] (
	id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- ���������� ������������� 
    [type] VARCHAR(50) NOT NULL,-- ��� �������
)

CREATE TABLE [board] (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- ���������� ������������� �����
    title NVARCHAR(255), -- �������� �����
	createdAt DATETIME2 DEFAULT GETDATE(),
	userId UNIQUEIDENTIFIER, -- ������������� ���������

	designTypeId UNIQUEIDENTIFIER, -- ��� �������: ����, �����������, �������� � �.�.
    colorCode VARCHAR(10), -- ��� ����� �����
    imageFileId UNIQUEIDENTIFIER, -- ����������� �����
    
	isPublic BIT NOT NULL DEFAULT 0, -- �� ��������� ����� ���������
    generalAccessLevelId UNIQUEIDENTIFIER, --���������� ��� ����

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
    PRIMARY KEY (boardId, userId) -- ���������� ���� ��������� �� boardId � userId
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
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- ���������� ������������� �������
    title NVARCHAR(255) NOT NULL, -- �������� �������

	boardId UNIQUEIDENTIFIER, -- ������������� �����
    prevCardListId UNIQUEIDENTIFIER, -- ������������� ���������� �������
    nextCardListId UNIQUEIDENTIFIER, -- ������������� ��������� �������
    FOREIGN KEY (boardId) REFERENCES board(id),
    FOREIGN KEY (prevCardListId) REFERENCES [cardList](id),
    FOREIGN KEY (nextCardListId) REFERENCES [cardList](id)
);

CREATE TABLE [card] (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- ���������� ������������� ��������
    title NVARCHAR(255) NOT NULL, -- ��������� ��������
    [description] NVARCHAR(MAX), -- �������� ��������

	imageFileId UNIQUEIDENTIFIER, -- ������������� �������
    cardListId UNIQUEIDENTIFIER, -- ������������� �������
    prevCardId UNIQUEIDENTIFIER, -- ������������� ���������� ��������
    nextCardId UNIQUEIDENTIFIER, -- ������������� ��������� ��������

	FOREIGN KEY (imageFileId) REFERENCES [file](id),
    FOREIGN KEY (cardListId) REFERENCES [cardList](id),
    FOREIGN KEY (prevCardId) REFERENCES [card](id),
    FOREIGN KEY (nextCardId) REFERENCES [card](id)
);

CREATE TABLE [cardAttachment](
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- ���������� ������������� ������
    fileId UNIQUEIDENTIFIER NOT NULL,
    cardId UNIQUEIDENTIFIER, -- ������������� ��������, � ������� ��������� ������
	createdAt DATETIME2 DEFAULT GETDATE(), -- ���� ��������
    FOREIGN KEY (cardId) REFERENCES [card](id),
	FOREIGN KEY (fileId) REFERENCES [file](id)
);

CREATE TABLE [cardComment] (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- ���������� ������������� �����������
    [text] NVARCHAR(MAX) NOT NULL, -- ����� �����������
	createdAt DATETIME2 DEFAULT GETDATE(), -- ���� ��������
	updatedAt DATETIME2 DEFAULT GETDATE(), -- ���� ����������	
    cardId UNIQUEIDENTIFIER, -- ������������� ��������, � ������� ��������� �����������
	userId UNIQUEIDENTIFIER, -- ������������� ������ �����������
    FOREIGN KEY (cardId) REFERENCES [card](id),
	FOREIGN KEY (userId) REFERENCES [user](id) -- ����� � 
);


INSERT INTO accessLevel (type) VALUES 
('reader'),('editor'),('owner');

INSERT INTO role (type) VALUES 
('user'),('admin');

INSERT INTO design (type) VALUES 
('color'),('image');