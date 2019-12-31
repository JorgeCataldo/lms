import { EventPreview } from 'src/app/models/previews/event.interface';
import { ModulePreview } from 'src/app/models/previews/module.interface';
import { TrackPreview } from 'src/app/models/previews/track.interface';
import { ProfileTestResponse } from 'src/app/models/profile-test.interface';

export const responseMock: ProfileTestResponse = {
  'id': '5c73f3098a0e5d10805b4ef6',
  'createdAt': new Date('2019-02-25T10:53:49.8443236-03:00'),
  'createdBy': '5c736b707a9c94781b9d2bf7',
  'userName': 'teste',
  'userRegisterId': null,
  'testId': '5c73f2dd8a0e5d10805b4ef3',
  'testTitle': 'Novo Teste',
  'answers': [{
    'questionId': '5c73f2dd8a0e5d10805b4ef4', 'question': 'Pergunta?', 'answer': 'Resposta', 'percentage': 50, 'grade': null
  }, {
    'questionId': '5c73f2de8a0e5d10805b4ef5', 'question': 'Pergunta 2?', 'answer': 'C', 'percentage': 50, 'grade': 0.0
  }],
  'recommended': false,
  'modulesInfo': [{ 'id': '5c646f1ce15797206584bd4d', 'name': 'Novo Módulo!' }],
  'tracksInfo': [{ 'id': '5c645dde5bc6504c751a14bf', 'name': 'Nova Trilha!' }],
  'eventsInfo': [{ 'id': '5c645dde5bc6504c751a1455', 'name': 'Novo Evento!' }]
};

export const eventsMock: Array<EventPreview> = [{
  'id': '5c13e97aab1c6871c5906330',
  'instructorId': '000000000000000000000000',
  'title': 'Case Advisory',
  'excerpt': 'Este case expõe o completo trabalho de um advisor(...)',
  'imageUrl': 'http://dev.academia.staging.api.tg4.com.br/track/252e5f57-c340-4937-9beb-f9c80745e069.png',
  'published': false,
  'nextSchedule': null,
  'schedules': [],
  'hasUserProgess': false,
  'requirements': []
}];

export const modulesMock: Array<ModulePreview> = [{
  'id': '5c6d5a9d6484113c901f7f29',
  'instructorId': '000000000000000000000000',
  'title': 'A New Module Modificado',
  'published': false,
  'excerpt': 'New Module Excerpt',
  'instructor': 'New Instructor',
  'imageUrl': '//localhost:5055/module/12d4548e-2365-4550-bdad-9fc406acf3f2.png',
  'tags': ['tag1', 'tag2'],
  'subjects': [],
  'requirements': [],
  'hasUserProgess': false
}];

export const tracksMock: Array<TrackPreview> = [{
  'id': '5c3f4f8071541766e6db2347',
  'title': 'FUNDOS E ALOCAÇÃO DE RECURSOS',
  'description': 'Engloba os módulos necessários',
  'imageUrl': 'http://dev.academia.staging.api.tg4.com.br/track/252e5f57-c340-4937-9beb-f9c80745e069.png',
  'recommended': false,
  'eventCount': 3,
  'moduleCount': 10,
  'duration': 45558.0,
  'published': false
}];
