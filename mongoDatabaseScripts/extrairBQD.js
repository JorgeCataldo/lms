const MongoClient = require('mongodb').MongoClient;
const assert = require('assert');

// documentaçÃo base
// https://github.com/mongodb/node-mongodb-native

// Connection URL
// const url = 'mongodb://dev:tecs638form@ds113732.mlab.com:13732/academia-btg-bkp';
// const url = 'mongodb://dev:tecs638form@ds161312.mlab.com:61312/academia-btg-bkp-03-12';
const url = 'mongodb://dev:tecs638form@ds149682.mlab.com:49682/academia-btg'; // 

// Database Name
const dbName = 'academia-btg';

const urls = [
    ['mongodb://dev:tecs638form@ds157873.mlab.com:57873/', '04-02' ],
    ['mongodb://dev:tecs638form@ds143678.mlab.com:43678/', '04-03' ],
    ['mongodb://dev:tecs638form@ds123718.mlab.com:23718/', '04-04' ],
    ['mongodb://dev:tecs638form@ds157873.mlab.com:57873/', '04-05' ],
    ['mongodb://dev:tecs638form@ds143678.mlab.com:43678/', '04-06' ],
    ['mongodb://dev:tecs638form@ds143678.mlab.com:43678/', '04-07' ],
    ['mongodb://dev:tecs638form@ds143678.mlab.com:43678/', '04-08' ],
]

var tablename = 'bdq_new';

async function getMongo(url, dbName, tablename) {
    // // Use connect method to connect to the server
    var client = await MongoClient.connect(url);

    console.log("Connected successfully to server");

    const db = client.db(dbName);
    //console.log(db);

    const collection = db.collection('Modules');
    // Find some documents
    var modules = await collection.find({}).toArray();

    const bdqCollection = db.collection('Questions');
    // Find some documents
    var quest = await bdqCollection.find({}).toArray();


    var excel = `
    create table ${tablename} (
    moduleid varchar(100),
    modulename varchar(max),
    subjectid varchar(100),
    subjectName varchar(max),
    questionid varchar(100),
    questionname varchar(max),
    answerid varchar(100),
    answername varchar(max))
    GO
    `;

    modules.forEach(function (mod, index) {
        var subjects = {};
        if (mod.subjects && mod.subjects.length > 0) {
            for (var i = 0; i < mod.subjects.length; i++) {
                subjects[mod.subjects[i]._id.toString()] = mod.subjects[i].title;
            }
        }
        var questions = quest.filter(function (q) {
            return q.moduleId.toString() == mod._id.toString();
        });
        if (questions && questions.length > 0) {
            for (var j = 0; j < questions.length; j++) {
                var question = questions[j];

                if (question.answers || question.answers.length > 0) {

                    var line = "insert into " + tablename + " values ('" + mod._id.toString() + "','" + mod.title + "','" +
                        question.subjectId.toString() + "','" + subjects[question.subjectId.toString()] + "','" +
                        question._id.toString() + "','" + (question.text ? question.text.replace(/\n/g, "<br />").replace(/'/g, "''") : "");

                    for (var k = 0; k < question.answers.length; k++) {
                        var answer = question.answers[k];
                        excel += line + "','" + answer._id.toString() + "','" + (answer.description ? answer.description.replace(/\n/g, "<br />").replace(/'/g, "''") : "") + "')\n";
                    }
                }
            }
        }
    });

    //console.log(excel);

    const fs = require('fs');
    fs.writeFile("extracao_" + tablename + ".sql", excel, function (err) {
        if (err) {
            return console.log(err);
        }

        console.log("The file was saved!");
    });
    console.log("acabou");

}
// urls.forEach(conn => {
//     var dbName = 'academia-btg-bkp-' + conn[1];
//     getMongo(conn[0] + dbName, dbName, 'bdq_old_' + conn[1].replace('-', '_'));
// });


async function getMongoAnswers() {
    // // Use connect method to connect to the server
    var client = await MongoClient.connect(url);

    console.log("Connected successfully to server");

    const db = client.db(dbName);
    //console.log(db);

    const collection = db.collection('UserSubjectProgress');
    // Find some documents
    var modules = await collection.find({}).toArray();

    var txt = "";

    modules.forEach(function (mod, index) {

        if (mod.answers && mod.answers.length > 0) {
            for (var j = 0; j < mod.answers.length; j++) {
                var answer = mod.answers[j];

                txt += "insert into answers values ('" + mod.moduleId.toString() + "','" + mod.subjectId.toString() + "','" +
                    mod.userId.toString() + "','" + answer.questionId.toString() + "','" +
                    (answer.answerId ? answer.answerId.toString() : "") + "','" + answer.answerDate[0].toString() + "')\n";
            }
        }
    });

    //console.log(excel);

    const fs = require('fs');
    fs.writeFile("respostas_" + tablename + ".sql", txt, function (err) {
        if (err) {
            return console.log(err);
        }

        console.log("The file was saved!");
    });
    console.log("acabou");

}
getMongoAnswers();


async function getMongoDraftAnswers() {
    // // Use connect method to connect to the server
    var client = await MongoClient.connect(url);

    console.log("Connected successfully to server");

    const db = client.db(dbName);

    const bdqCollection = db.collection('QuestionsDrafts');
    // Find some documents
    var questions = await bdqCollection.find({}).toArray();


    if (questions && questions.length > 0) {
        for (var j = 0; j < questions.length; j++) {
            var question = questions[j];

            if (question.answers || question.answers.length > 0) {

                var line = "insert into drafts values ('" + question.moduleId.toString() + "','" + question.subjectId.toString() + "','" +
                    question._id.toString() + "','" + (question.text ? question.text.replace(/\n/g, "<br />").replace(/'/g, "''") : "");

                for (var k = 0; k < question.answers.length; k++) {
                    var answer = question.answers[k];
                    excel += line + "','" + answer._id.toString() + "','" + (answer.description ? answer.description.replace(/\n/g, "<br />").replace(/'/g, "''") : "") + "')\n";
                }
            }
        }
    }

    //console.log(excel);

    const fs = require('fs');
    fs.writeFile("drafts_" + tablename + ".sql", excel, function (err) {
        if (err) {
            return console.log(err);
        }

        console.log("The file was saved!");
    });
    console.log("acabou");

}
//getMongoDraftAnswers();

// var sql = require('mssql');

// const config = {
//     user: 'sa',
//     password: 'dvd3000',
//     server: 'localhost',
//     database: 'bdq'
// };

// async function aaaa() {
//     var pool = await sql.connect(config);
//     const request = pool.request()
//     request.query('select 1000', (err, result) => {
//         console.dir(result);
//     });
// }

// aaaa();