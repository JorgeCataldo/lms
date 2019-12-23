import { MenuSection } from './menu-item.interface';
import { environment } from 'src/environments/environment';

export function allMenuItems(): Array<MenuSection> {
  const menu = [{
    title: 'Início',
    items: [{
      url: 'home', title: 'Home',
      iconClass: 'icon icon-home sidemenu-icon',
      permittedRoles: ['Student', 'Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }, /*{
      url: 'performance', title: 'Desempenho',
      iconClass: 'icon icon-processos_sincronia sidemenu-icon',
      permittedRoles: ['Student'],
      checkProfileTest: false, checkFormulas: false
    },*/
    {
      url: 'catalogo-de-cursos', title: 'Catálogo de Cursos',
      iconClass: 'icon icon-trilhas sidemenu-icon',
      permittedRoles: ['Student', 'Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: environment.features.eCommerce
    }, {
      url: 'configuracoes/detalhes-usuario/:userId', title: 'Meus Dados',
      iconClass: 'icon icon-trilhas sidemenu-icon',
      permittedRoles: ['Student', 'Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true,
      blockAccess: ['BusinessManagerStudent']
    }, /*, {
      url: 'atendimento', title: 'Mercado',
      iconClass: 'icon icon-trilhas sidemenu-icon',
      permittedRoles: ['Student'],
      checkProfileTest: false, checkFormulas: false
    }*/, {
      url: 'atendimento', title: 'Suporte & Atendimento',
      iconClass: 'icon icon-atendimento sidemenu-icon',
      permittedRoles: ['Student', 'Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }],
    permittedRoles: ['Student', 'Admin'],
    isRunningFeature: true
  }, {
    title: 'Aprendizagem',
    items: [{
      url: 'meus-cursos', title: 'Meus Cursos e Avaliações',
      iconClass: 'icon icon-modulo sidemenu-icon',
      permittedRoles: ['Student', 'Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }, {
      url: 'home/calendario', title: 'Calendário de Atividades',
      iconClass: 'icon icon-eventos sidemenu-icon',
      permittedRoles: ['Student', 'Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }, {
      url: 'performance', title: 'Meu Desempenho',
      iconClass: 'icon icon-processos_sincronia sidemenu-icon',
      permittedRoles: ['Student', 'Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }],
    permittedRoles: ['Student', 'Admin'],
    isRunningFeature: true
  }, {
    title: 'Desenvolvimento Profissional',
    items: [{
      url: 'configuracoes/card-recomendacao/:userId', title: 'Meu Perfil Profissional',
      iconClass: 'icon icon-gerenciar_equipe sidemenu-icon',
      permittedRoles: ['Student', 'Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: environment.features.career,
      blockAccess: ['BusinessManagerStudent']
    }, /*{
      url: 'minha-candidatura', title: 'Minhas Oportunidades',
      iconClass: 'icon icon-trilhas sidemenu-icon',
      permittedRoles: ['Student', 'Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: environment.features.career,
      blockAccess: ['BusinessManagerStudent']
    }*/, {
      url: 'buscar-vagas-aluno', title: 'Oportunidades',
      iconClass: 'icon icon-trilhas sidemenu-icon',
      permittedRoles: ['Student', 'Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: environment.features.career,
      blockAccess: ['BusinessManagerStudent']
    }],
    permittedRoles: ['Student', 'Admin'],
    isRunningFeature: environment.features.career
  }, {
    title: 'Gerenciamento de Programas',
    items: [{
      url: 'configuracoes/enturmacao-matricula', title: 'Enturmação e Matrícula',
      iconClass: 'icon icon-trilhas sidemenu-icon',
      permittedRoles: ['Admin', 'Secretary'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }, {
      url: 'configuracoes/gerenciar-equipe', title: 'Gerenciar Matrículas',
      iconClass: 'icon icon-gerenciar_equipe sidemenu-icon',
      permittedRoles: ['Admin', 'Secretary'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }, {
      url: 'configuracoes/emails-enviados', title: 'Gerenciar Contatos',
      iconClass: 'icon icon-historico_emails sidemenu-icon',
      permittedRoles: ['Admin', 'Secretary'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }],
    permittedRoles: ['Admin', 'Secretary'],
    isRunningFeature: true
  }, {
    title: 'Acompanhamento de Programas',
    items: [{
      url: 'empenho-desempenho', title: 'Empenho e Desempenho',
      iconClass: 'icon icon-trilhas sidemenu-icon',
      permittedRoles: ['Admin', 'Secretary'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }, {
      url: 'configuracoes/resultado-testes-de-avaliacao', title: 'Resultado de Avaliações nos Cursos',
      iconClass: 'icon icon-gerenciar_equipe sidemenu-icon',
      permittedRoles: ['Admin', 'Secretary'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }, {
      url: 'configuracoes/resultado-pesquisa-na-base', title: 'Resultado de Pesquisas',
      iconClass: 'icon icon-gerenciar_equipe sidemenu-icon',
      permittedRoles: ['Admin', 'Secretary'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }, {
      url: 'configuracoes/resultado-pesquisa-na-base-nps', title: 'Resultado de Pesquisas - NPS',
      iconClass: 'icon icon-gerenciar_equipe sidemenu-icon',
      permittedRoles: ['Admin', 'Secretary'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }, {
      url: 'relatorios', title: 'Relatórios Analíticos',
      iconClass: 'icon icon-gerenciar_equipe sidemenu-icon',
      permittedRoles: ['Admin', 'Secretary'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }],
    permittedRoles: ['Admin', 'Secretary'],
    isRunningFeature: true
  }, {
    title: 'Gestão de Competências',
    items: [{
      url: 'mapa-de-competencias', title: 'Mapa de Competências',
      iconClass: 'icon icon-trilhas sidemenu-icon',
      permittedRoles: ['Admin', 'HumanResources', 'Recruiter'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: environment.features.recruitment
    }, {
      url: 'desempenho-da-capacitacao', title: 'Desempenho da Capacitação',
      iconClass: 'icon icon-gerenciar_equipe sidemenu-icon',
      permittedRoles: ['Admin', 'HumanResources', 'Recruiter'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: environment.features.recruitment
    }, {
      url: 'historico-de-performance-profissional', title: 'Histórico de Performance Profissional',
      iconClass: 'icon icon-gerenciar_equipe sidemenu-icon',
      permittedRoles: ['Admin', 'HumanResources', 'Recruiter'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: environment.features.recruitment
    }],
    permittedRoles: ['Admin', 'HumanResources', 'Secretary', 'Student', 'Recruiter', 'BusinessManager'],
    isRunningFeature: environment.features.recruitment
  }, {
    title: 'Talentos & Oportunidades',
    items: [{
      url: 'configuracoes/vagas-empresa', title: 'Gerenciar Oportunidades',
      iconClass: 'icon icon-trilhas sidemenu-icon',
      permittedRoles: ['Admin', 'HumanResources', 'Recruiter'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: environment.features.recruitment
    }, {
      url: 'configuracoes/buscar-talentos', title: 'Banco de Talentos',
      iconClass: 'icon icon-trilhas sidemenu-icon',
      permittedRoles: ['Admin', 'HumanResources', 'Recruiter'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: environment.features.recruitment
    }],
    permittedRoles: ['Admin', 'HumanResources', 'Recruiter'],
    isRunningFeature: environment.features.recruitment
  }, {
    title: 'Desenho Instrucional',
    items: [{
      url: 'configuracoes/modulos', title: 'Cursos e Avaliações',
      iconClass: 'icon icon-modulo sidemenu-icon',
      permittedRoles: ['Admin', 'Author'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true,
      checkAccess: ['Modules']
    }, {
      url: 'configuracoes/eventos', title: 'Eventos',
      iconClass: 'icon icon-eventos sidemenu-icon',
      permittedRoles: ['Admin', 'Author'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true,
      checkAccess: ['Events']
    }, {
      url: 'configuracoes/trilhas', title: 'Trilhas',
      iconClass: 'icon icon-trilhas sidemenu-icon',
      permittedRoles: ['Admin', 'Author'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true,
      checkAccess: ['Tracks']
    } /*, {
      url: 'configuracoes/testes-de-avaliacao', title: 'Avaliações',
      iconClass: 'icon icon-usuarios sidemenu-icon',
      permittedRoles: ['Student', 'BusinessManager'],
      checkProfileTest: false, checkFormulas: false,
      checkAccess: ['Gestor']
    }, {
      url: 'configuracoes/formulas', title: 'Fórmulas',
      iconClass: 'icon icon-gerenciar_equipe sidemenu-icon',
      permittedRoles: ['Admin'],
      checkProfileTest: false, checkFormulas: true, isRunningFeature: environment.features.formulas
    }*/],
    permittedRoles: ['Admin', 'HumanResources', 'Secretary', 'Student', 'Recruiter', 'BusinessManager', 'Author'],
    isRunningFeature: true
  }, {
    title: 'Avaliações e Pesquisas',
    items: [/*{
      url: 'banco-questoes-tags', title: 'Banco de Questões & Tags',
      iconClass: 'icon icon-usuarios sidemenu-icon',
      permittedRoles: ['Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    },*/ {
      url: 'configuracoes/testes-de-avaliacao', title: 'Avaliações e Pesquisas nos Cursos',
      iconClass: 'icon icon-usuarios sidemenu-icon',
      permittedRoles: ['Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }, {
      url: 'configuracoes/pesquisa-na-base', title: 'Pesquisas na Base',
      iconClass: 'icon icon-usuarios sidemenu-icon',
      permittedRoles: ['Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: environment.features.profileTest
    }, {
      url: 'configuracoes/pesquisa-na-base-nps', title: 'Pesquisas na Base - NPS',
      iconClass: 'icon icon-usuarios sidemenu-icon',
      permittedRoles: ['Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }],
    permittedRoles: ['Admin', 'HumanResources', 'Secretary', 'Student', 'Recruiter', 'BusinessManager'],
    isRunningFeature: true
  }, {
    title: 'Cadastro',
    items: [{
      url: 'configuracoes/gerenciar-equipe', title: 'Gerenciar Usuários',
      iconClass: 'icon icon-processos_sincronia sidemenu-icon',
      permittedRoles: ['Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }, {
      url: 'configuracoes/processos-de-sincronia', title: 'Carregamento em Lote',
      iconClass: 'icon icon-processos_sincronia sidemenu-icon',
      permittedRoles: ['Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }],
    permittedRoles: ['Admin'],
    isRunningFeature: true
  }, {
    title: 'Parametrização',
    items: [{
      url: 'home/color', title: 'Paletas de cores',
      iconClass: 'icon icon-gerenciar_equipe sidemenu-icon',
      permittedRoles: ['Admin', 'BusinessManager'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: environment.features.colorPalette
    }],
    permittedRoles: ['Admin', 'HumanResources', 'Secretary', 'Student', 'Recruiter', 'BusinessManager'],
    isRunningFeature: environment.features.colorPalette
  }/*, {
    title: 'Relatórios Analíticos',
    items: [{
      url: 'configuracoes/logs', title: 'Logs de Acesso',
      iconClass: 'icon icon-gerenciar_equipe sidemenu-icon',
      permittedRoles: ['Admin'],
      checkProfileTest: false, checkFormulas: false, isRunningFeature: true
    }],
    permittedRoles: ['Admin'],
    isRunningFeature: true
  }*/];
  // if (environment.features.recruitment) {
  //   menu.push({
  //     title: 'Recrutamento',
  //     items: [{
  //       url: 'configuracoes/identificacao-empresa', title: 'Perfil da Empresa',
  //       iconClass: 'icon icon-usuarios sidemenu-icon',
  //       permittedRoles: ['Recruiter'],
  //       checkProfileTest: false, checkFormulas: false
  //     }, {
  //       url: 'configuracoes/buscar-talentos', title: 'Buscar Talentos',
  //       iconClass: 'icon icon-trilhas sidemenu-icon',
  //       permittedRoles: ['Admin', 'HumanResources'],
  //       checkProfileTest: false, checkFormulas: false
  //     }, {
  //       url: 'configuracoes/vagas-empresa', title: 'Minhas Vagas',
  //       iconClass: 'icon icon-trilhas sidemenu-icon',
  //       permittedRoles: ['Recruiter'],
  //       checkProfileTest: false, checkFormulas: false
  //     }],
  //     permittedRoles: ['Admin', 'HumanResources', 'Recruiter']
  //   });
  // }
  return menu;
}
