import { Requirement } from 'src/app/settings/modules/new-module/models/new-requirement.model';
import { Content } from 'src/app/models/content.model';
import { Subject } from 'src/app/models/subject.model';

export const contentsMock: Array<Content> = [{
  'id': '5c6d5e0b6484113c901f7f31',
  'title': 'Content',
  'excerpt': 'Content Description',
  'duration': 283,
  'referenceUrls': ['google.com.br'],
  'concepts': [{'name': 'concept', 'positions': [60], 'anchors': null}],
  'value': 'https://player.vimeo.com/video/42953236',
  'type': 0,
  'numPages': null
}];

export const subjectsMock: Array<Subject> = [{
  'id': '5c6d5dc06484113c901f7f2f',
  'concepts': ['concept'],
  'contents': contentsMock,
  'userProgresses': [{'level': 0, 'percentage': 1.0}, {'level': 1, 'percentage': 1.0}, {'level': 2, 'percentage': 1.0}],
  'hasQuestions': false,
  'title': 'Subject',
  'excerpt': 'Subject excerpt'
}];

export const requirementsMock: Array<Requirement> = [{
  'moduleId': '5c13cfc4ab1c6871c59062ad',
  'title': 'Advisory',
  'optional': false,
  'level': 0,
  'percentage': 1.0,
  'requirementValue': null
}];

export const moduleMock = {
  'id': '5c6d5a9d6484113c901f7f29',
  'title': 'A New Module Modificado',
  'published': false,
  'excerpt': 'New Module Excerpt',
  'instructor': 'New Instructor',
  'instructorMiniBio': 'Instructor Biography',
  'instructorImageUrl': '//localhost:5055/module/9cdcf15d-d857-4780-88d4-22b3e9eff948.png',
  'instructorId': '000000000000000000000000',
  'imageUrl': '//localhost:5055/module/12d4548e-2365-4550-bdad-9fc406acf3f2.png',
  'videoUrl': 'https://player.vimeo.com/video/42953236',
  'duration': 0,
  'videoDuration': 283,
  'tags': ['tag1', 'tag2'],
  'supportMaterials': [{
    'id': '5c6d62a84aa724221cbab12d',
    'title': 'Support Material',
    'description': 'Google it',
    'downloadLink': 'https://www.google.com.br/',
    'imageUrl': null,
    'type': 1
  }, {
    'id': '5c6d62a84aa724221cbab12e',
    'title': 'Support Material 2',
    'description': 'Support material',
    'downloadLink': '//localhost:5055/module/3c95297a-83b8-4e5d-a046-28ffdb869b65.jpg',
    'imageUrl': null,
    'type': 2
  }],
  'subjects': subjectsMock,
  'requirements': requirementsMock,
  'certificateUrl': null,
  'tutorsIds': ['5c64720fe15797206584bd4e'],
  'tutors': [{'id': '5c64720fe15797206584bd4e', 'name': 'Leonardo Giroto', 'imageUrl': ''}],
  'storeUrl': 'https://www.google.com.br/',
  'questionsLimit': null,
  'ecommerceId': null
};
