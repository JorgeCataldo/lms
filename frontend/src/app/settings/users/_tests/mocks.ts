import { UserCareer, Institute } from '../user-models/user-career';
import { UserRecommendation } from 'src/app/models/user-recommendation.model';
import { LoginResponse } from 'src/app/shared/services/auth.service';

export const careerInfoMock: { id: string, career: UserCareer } = {
  'id': '5b9d276606bd6663cc6508ba',
  'career': {
    'professionalExperience': false,
    'professionalExperiences': [],
    'colleges': [{
      'instituteId': '5c5c9d0678a3262c08bc7074',
      'title': 'Puc',
      'name': 'Pão de alho',
      'status': 'Completo',
      'startDate': new Date('2019-02-01T02:00:00+00:00'),
      'endDate': new Date('2019-02-28T03:00:00+00:00'),
      'cr': '9',
      'campus': '',
      'academicDegree': '',
      'completePeriod': 'Segundo Semestre'
    }, {
      'instituteId': '5c5c9d0678a3262c08bc7074',
      'title': 'Puc',
      'name': 'Bolo',
      'status': 'Cursando',
      'startDate': null,
      'endDate': null,
      'cr': '1',
      'campus': '',
      'academicDegree': '',
      'completePeriod': 'Segundo Semestre'
    }],
    'rewards': [{'title': 'Lord do universo', 'name': 'Tadeu', 'link': '', 'date': new Date('2019-02-13T02:00:00+00:00') }],
    'languages': [{
      'names': 'Italiano', 'languages': 'ingles', 'level': 'Intermediário'
    }, {
      'names': 'Italiano', 'languages': 'frances', 'level': 'Avançado'
    }, {
      'names': 'Holandes', 'languages': 'espanhol ', 'level': 'Intermediário'
    }, {
      'names': 'Holandes', 'languages': 'portugues', 'level': 'Avançado'
    }, {
      'names': 'Holandes', 'languages': 'alemao', 'level': 'Básico'
    }, {
      'names': 'Italiano', 'languages': 'outro', 'level': 'Intermediário'
    }],
    'abilities': [{'name': 'Dançar', 'hasLevel': true , 'level': 'Avançado'}],
    'certificates': [],
    'skills': [],
    'travelAvailability': true,
    'movingAvailability': true
  }
};

export const userInfoMock = {
 'id': '5b9d276606bd8273cc6508ba',
 'userName': 'admin',
 'name': 'Aluno 1',
 'email': 'felipe@tg4.com.br',
 'phone': '21980808080',
 'isBlocked': false,
 'isEmailConfirmed': false,
 'cpf': {'value': '11111111111'},
 'address': null,
 'imageUrl': 'http://dev.academia.staging.api.tg4.com.br/module/39b2819d-aa33-4c50-9190-3a8ddde90302.png',
 'responsibleId': '5b9d276606bd8273cc6508ba',
 'registrationId': '',
 'info': '',
 'modulesInfo': [],
 'tracksInfo': [{'id': '5c1bad71bab74011fd45091d',
 'title': 'MASTER IN FINANCIAL MARKETS - JANEIRO 2019',
 'imageUrl': 'http://dev.academia.staging.api.tg4.com.br/track/2b217e59-4346-412b-8d3c-241cd0aa04f3.png',
 'level': 0, 'percentage': 0.0, 'blocked': false}],
 'eventsInfo': [],
 'acquiredKnowledge': null,
 'lineManager': 'Aluno 1',
 'lineManagerEmail': null,
 'serviceDate': '0001-01-01T00:00:00+00:00',
 'education': null,
 'gender': null,
 'manager': null,
 'cge': null,
 'idCr': null,
 'coHead': null,
 'company': null,
 'businessGroup': null,
 'businessUnit': null,
 'country': null,
 'language': null,
 'frontBackOffice': null,
 'job': null,
 'location': null,
 'rank': null,
 'sectors': [],
 'segment': null,
 'role': 'Admin',
 'cep': null,
 'city': null,
 'uf': null,
 'street': null,
 'number': null,
 'neighborhood': null,
 'complement': null
};

export const loggedUserMock: LoginResponse = {
// tslint:disable-next-line: max-line-length
  'access_token': 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjViOWQyNzY2MDZiZDgyNzNjYzY1MDhiYSIsImp0aSI6Ijc4NTRiMjc3LTMzZDgtNDE5NC1iOWIxLTBjYzgwN2MxODk3YiIsImlhdCI6MTU1MjU3MDgzMiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE1NTI1NzA4MzIsImV4cCI6MTU1Mjc4NjgzMiwiaXNzIjoiQWNhZGVtaWFDQSIsImF1ZCI6IkFjYWRlbWlhQ0FBdWRpZW5jZSJ9.potwYOnJDFXVEwRo2w426Kh47Eg5k8isJRT_HvfNbUU',
  'expires_in': 216000,
  'refresh_token': 'c953b36e-5b9a-4f2f-bd08-6e1cb64fd3fd',
  'name': 'Aluno 1',
  'username': 'admin',
  'user_id': '5b9d276606bd6663cc6508ba',
  'role': 'Admin',
  'completed_registration': true,
  'first_access': false,
  'email_verified': true
};

export const careerInstitutesMock: Institute[] = [
  { 'name': 'Ph', 'type': 1 }
];

export const userRecommendationMock: any = {
  'userInfo': {
    'imageUrl': 'http://dev.academia.api.tg4.com.br/module/c1f06aae-724e-44d5-aff9-ed8663004cf8.png',
    'name': 'Aluno 0',
    'dateBorn': null,
    'address': {
      'street': 'string',
      'address2': 'string',
      'district': 'string',
      'city': 'string',
      'state': 'string',
      'country': 'string',
      'zipCode': 'string'
    },
    'email': 'leonardo@tg4.com.br',
    'phone': '21994098838',
    'phone2': null,
    'linkedIn': 'https://www.linkedin.com/in/leonardo-giroto-aa8ba879/',
    'profile': null,
    'allowRecommendation': true
  },
  'userCareer': {
    'professionalExperience': true,
    'professionalExperiences': [],
    'colleges': [],
    'rewards': [],
    'languages': [],
    'abilities': [],
    'certificates': [],
    'skills': [],
    'travelAvailability': false,
    'movingAvailability': false,
    'shortDateObjectives': 'Esse é meu objetivo de curto prazo',
    'longDateObjectives': 'Esse é meu objetivo de longo prazo',
    'id': '5c82bfb59a515d264cdd60f1'
  },
  'userEventApplications': [{
    'eventId': '5c13e97aab1c6871c5906330',
    'eventName': 'Case Advisory',
    'faGrade': 86.2,
    'qcGrade': 85,
    'tgGrade': 86.2,
    'userGradeBaseValues': [
      { key: 'Auto_QC', value: '10.0' },
      { key: 'Auto_TG', value: '10.0' },
      { key: 'Auto_FA', value: '10.0' },
      { key: 'Intragrupo_QC', value: '9.0' },
      { key: 'Intragrupo_TG', value: '9.2' },
      { key: 'Intragrupo_FA', value: '9.2' },
      { key: 'Nota_Professor_Grupo', value: '7.0' },
      { key: 'Nota_Final', value: '8.6' }
    ]
  }],
  'currentUser': true,
  'canFavorite': false,
  'isFavorite': false
};
