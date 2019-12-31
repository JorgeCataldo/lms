create table bdq_old_12_03 (
moduleid varchar(100),
modulename varchar(max),
subjectid varchar(100),
subjectName varchar(max),
questionid varchar(100),
questionname varchar(max),
answerid varchar(100),
answername varchar(max))

create table bdq_old_29_03 (
moduleid varchar(100),
modulename varchar(max),
subjectid varchar(100),
subjectName varchar(max),
questionid varchar(100),
questionname varchar(max),
answerid varchar(100),
answername varchar(max))

create table bdq_new (
moduleid varchar(100),
modulename varchar(max),
subjectid varchar(100),
subjectName varchar(max),
questionid varchar(100),
questionname varchar(max),
answerid varchar(100),
answername varchar(max))

create table drafts (
moduleid varchar(100),
subjectid varchar(100),
questionid varchar(100),
questionname varchar(max),
answerid varchar(100),
answername varchar(max))

delete from bdq_old_29_03

drop table [dbo].[answers]
CREATE TABLE [dbo].[answers](
	[moduleid] [varchar](100) NULL,
	[subjectid] [varchar](100) NULL,
	[userid] [varchar](100) NULL,
	[questionid] [varchar](100) NULL,
	[answerid] [varchar](100) NULL,
	answerDate [varchar](100) null,
) ON [PRIMARY]
GO

