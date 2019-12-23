import { ProfileTest, ProfileTestResponse } from 'src/app/models/profile-test.interface';

export const profileTestsMock: Array<ProfileTest> = [{
  'id': '5c59d01522e81f2b14e17149',
  'title': 'Teste de Perfil',
  'questions': [
    '5c631c4c887d3327d47902da',
    '5c631cb7887d3327d47902db',
    '5c631cb7887d3327d47902dc',
    '5c631cb7887d3327d47902dd'
  ]
}];

export const profileTestMock: ProfileTest = {
  'id': '5c59d01522e81f2b14e17149',
  'title': 'Teste de Perfil',
  'isDefault': true,
  'testQuestions': [{
    'id': '5c6ea1111f6d2736ec5f95ce',
    'testId': '5c59d01522e81f2b14e17149',
    'title': 'Pergunta 1?',
    'percentage': 50,
    'type': 2,
    'options': null
  }, {
    'id': '5c6ef37b5e09cc09fcf9f513',
    'testId': '5c59d01522e81f2b14e17149',
    'title':
    'Pergunta 2?',
    'percentage': 50,
    'type': 1,
    'options': [
      {'text': 'A', 'correct': true},
      {'text': 'B', 'correct': false},
      {'text': 'C', 'correct': false},
      {'text': 'D', 'correct': false}]
    }
  ]
};

export const profileTestResponsesMock: Array<ProfileTestResponse> = [{
  'id': '5c73f3098a0e5d10805b4ef6',
  'createdAt': new Date(),
  'userName': 'teste',
  'userRegisterId': null,
  'answers': [{
    'questionId': '5c73f2dd8a0e5d10805b4ef4', 'question': 'Pergunta?', 'answer': 'Resposta', 'grade': 40.0
  }, {
    'questionId': '5c73f2de8a0e5d10805b4ef5', 'question': 'Pergunta 2?', 'answer': 'C', 'grade': 0.0
  }]
}];
