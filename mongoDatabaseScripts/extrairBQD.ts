
const MongoClient = require('mongodb').MongoClient;
const assert = require('assert');
var sql = require('mssql');

// Connection URL
//const url = 'mongodb://dev:tecs638form@ds113732.mlab.com:13732/academia-btg-bkp';
//const url = 'mongodb://dev:tecs638form@ds161312.mlab.com:61312/academia-btg-bkp-03-12';
const url = 'mongodb://dev:tecs638form@ds149682.mlab.com:49682/academia-btg';// 
// Database Name
const dbName = 'academia-btg';

const config = {
    user: 'sa',
    password: 'dvd3000',
    server: '.',
    database: 'bdq'
};

(async function () {
    let pool = await sql.connect(config);
    let mongoClient = MongoClient.connect(url);
    const db = mongoClient.db(dbName);

    const collection = db.collection('Modules');
    const modules = await db.collection('Modules').find({});
    const quest = await db.collection('Questions').find({});

    let excel = "";

    for (var modIdx = 0; modIdx < modules.length; modIdx++) {
        var mod = modules[modIdx];
    }
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

                var line = mod._id.toString() + "\t" + mod.title + "\t" +
                    question.subjectId.toString() + "\t" + subjects[question.subjectId.toString()] + "\t" +
                    question._id.toString() + "\t" + (question.text ? question.text.replace(/\n/g, "<br />") : "");

                for (var k = 0; k < question.answers.length; k++) {
                    var answer = question.answers[k];
                    excel += line + "\t" + answer._id.toString() + "\t" + (answer.description ? answer.description.replace(/\n/g, "<br />") : "") + "\n";
                }
            }
        }
    }

    const fs = require('fs');
    fs.writeFile("extracao4.txt", excel, function (err) {
        if (err) {
            return console.log(err);
        }

        console.log("The file was saved!");
    });
    console.log("acabou");

})()
