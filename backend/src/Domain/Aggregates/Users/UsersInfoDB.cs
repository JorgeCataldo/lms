using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Aggregates.Users
{
    public class UserInfoDB
    {
        [Column(1)]
        public string Crit1 { get; set; }

        [Column(2)]
        public string Crit2 { get; set; }

        [Column(3)]
        public string Crit3 { get; set; }

        [Column(4)]
        public string Crit4 { get; set; }

        [Column(5)]
        public string Crit5 { get; set; }

        [Column(6)]
        public string Crit6 { get; set; }

        [Column(7)]
        public string Crit7 { get; set; }

        [Column(8)]
        public string Crit8 { get; set; }

        [Column(9)]
        public string Crit9 { get; set; }

        [Column(10)]
        public string Crit10 { get; set; }

        [Column(11)]
        public string Crit11 { get; set; }

        [Column(12)]
        public string Crit12 { get; set; }

        [Column(13)]
        public string Crit13 { get; set; }

        [Column(14)]
        public string Crit14 { get; set; }

        [Column(15)]
        public string Crit15 { get; set; }

        [Column(16)]
        public string Crit16 { get; set; }

        [Column(17)]
        public string Crit17 { get; set; }

        [Column(18)]
        public string Crit18 { get; set; }

        [Column(19)]
        public string Crit19 { get; set; }

        [Column(20)]
        public string Crit20 { get; set; }

        [Column(21)]
        public string Crit21 { get; set; }

        [Column(22)]
        public string Crit22 { get; set; }

        [Column(23)]
        public string Crit23 { get; set; }

        [Column(24)]
        public string Crit24 { get; set; }

        [Column(25)]
        public string Crit25 { get; set; }

        [Column(26)]
        public string Crit26 { get; set; }

        [Column(27)]
        public string Crit27 { get; set; }

        [Column(28)]
        public string Crit28 { get; set; }

        [Column(29)]
        public string Crit29 { get; set; }

        [Column(30)]
        public string Crit30 { get; set; }

        [Column(31)]
        public string Nota1 { get; set; }

        [Column(32)]
        public string Nota2 { get; set; }

        [Column(33)]
        public string Nota3 { get; set; }

        [Column(34)]
        public string Nota4 { get; set; }

        [Column(35)]
        public string Nota5 { get; set; }

        [Column(36)]
        public string Nota6 { get; set; }

        [Column(37)]
        public string Nota7 { get; set; }

        [Column(38)]
        public string Nota8 { get; set; }

        [Column(39)]
        public string Nota9 { get; set; }

        [Column(40)]
        public string Nota10 { get; set; }

        [Column(41)]
        public string Nota11 { get; set; }

        [Column(42)]
        public string Nota12 { get; set; }

        [Column(43)]
        public string Nota13 { get; set; }

        [Column(44)]
        public string Nome { get; set; }
        [Column(45)]
        public string Sobrenome { get; set; }
        [Column(46)]
        public string TipoPessoa { get; set; }
        [Column(47)]
        public string CPF { get; set; }
        [Column(48)]
        public string CNPJ { get; set; }
        [Column(49)]
        public string NomeEmpresa { get; set; }
        [Column(50)]
        public string Pais { get; set; }
        [Column(51)]
        public string CEP { get; set; }
        [Column(52)]
        public string Email { get; set; }
        [Column(53)]
        public string Endereco { get; set; }
        [Column(54)]
        public string Numero { get; set; }
        [Column(55)]
        public string Bairro { get; set; }
        [Column(56)]
        public string Cidade { get; set; }
        [Column(57)]
        public string Estado { get; set; }
        [Column(58)]
        public string Telefone { get; set; }
        [Column(59)]
        public string Celular { get; set; }
        [Column(60)]
        public string NomeCompleto { get; set; }

        [Column(61)]
        public string CompraDataPgto { get; set; }
        [Column(62)]
        public string CompraIdObj { get; set; }
        [Column(63)]
        public string CompraDataPgto2 { get; set; }
        [Column(64)]
        public string CompraIdObj2 { get; set; }

        [Column(65)]
        public string Matricula { get; set; }
        [Column(66)]
        public string Linkedin { get; set; }
        [Column(67)]
        public string NomeSocial { get; set; }
        [Column(68)]
        public string CPFAluno { get; set; }
        [Required]
        [Column(69)]
        public string EmailAluno { get; set; }
        [Column(70)]
        public string Senha { get; set; }
        [Column(71)]
        public string NovaSenha { get; set; }
        [Column(72)]
        public string NovaSenhaConfirm { get; set; }
        [Column(73)]
        public string Foto { get; set; }
        [Required]
        [Column(74)]
        public string NomeCompletoAluno { get; set; }
        [Column(75)]
        public string EnderecoAluno { get; set; }
        [Column(76)]
        public string ComplementoAluno { get; set; }
        [Column(77)]
        public string BairroAluno { get; set; }
        [Column(78)]
        public string CidadeAluno { get; set; }
        [Column(79)]
        public string EstadoAluno { get; set; }
        [Column(80)]
        public string Telefone1Aluno { get; set; }
        [Column(81)]
        public string Telefone2Aluno { get; set; }
        [Column(82)]
        public string DataNascimentoAluno { get; set; }
        [Column(83)]
        public string TipoDocAluno { get; set; }
        [Column(84)]
        public string NumDocAluno { get; set; }
        [Column(85)]
        public string OrgaoDocAluno { get; set; }
        [Column(86)]
        public string ValidadeDocAluno { get; set; }
        [Column(87)]
        public string DataEmissaoDocAluno { get; set; }
        [Column(88)]
        public string DocumentoIdentificacao { get; set; }
        [Column(89)]
        public string HistoricoEscolar { get; set; }
        [Column(90)]
        public string Mudanca { get; set; }
        [Column(91)]
        public string Deficiencia { get; set; }
        [Column(92)]
        public string DescricaoDeficiencia { get; set; }

        [Column(93)]
        public string GrauCurso1 { get; set; }
        [Column(94)]
        public string NomeCurso1 { get; set; }
        [Column(95)]
        public string Instituicao1 { get; set; }
        [Column(96)]
        public string AnoInicio1 { get; set; }
        [Column(97)]
        public string AnoConclusao1 { get; set; }
        [Column(98)]
        public string PeriodoAnoConclusao1 { get; set; }
        [Column(99)]
        public string CRAcumulado1 { get; set; }
        [Column(100)]
        public string SituacaoCurso1 { get; set; }
        [Column(101)]
        public string Campus1 { get; set; }

        [Column(102)]
        public string GrauCurso2 { get; set; }
        [Column(103)]
        public string NomeCurso2 { get; set; }
        [Column(104)]
        public string Instituicao2 { get; set; }
        [Column(105)]
        public string AnoInicio2 { get; set; }
        [Column(106)]
        public string AnoConclusao2 { get; set; }
        [Column(107)]
        public string PeriodoAnoConclusao2 { get; set; }
        [Column(108)]
        public string CRAcumulado2 { get; set; }
        [Column(109)]
        public string SituacaoCurso2 { get; set; }
        [Column(110)]
        public string Campus2 { get; set; }

        [Column(111)]
        public string GrauCursoN { get; set; }
        [Column(112)]
        public string NomeCursoN { get; set; }
        [Column(113)]
        public string InstituicaoN { get; set; }
        [Column(114)]
        public string AnoInicioN { get; set; }
        [Column(115)]
        public string AnoConclusaoN { get; set; }
        [Column(116)]
        public string PeriodoAnoConclusaoN { get; set; }
        [Column(117)]
        public string CRAcumuladoN { get; set; }
        [Column(118)]
        public string SituacaoCursoN { get; set; }
        [Column(119)]
        public string CampusN { get; set; }

        [Column(120)]
        public string Ingles { get; set; }
        [Column(121)]
        public string Excel { get; set; }
        [Column(122)]
        public string VBA { get; set; }
        [Column(123)]
        public string SoftwareEstatistica { get; set; }
        [Column(124)]
        public string PacoteOffice { get; set; }
        [Column(125)]
        public string PremiosReconhecimentos { get; set; }
        [Column(126)]
        public string Certificacoes { get; set; }
        [Column(127)]
        public string Idiomas { get; set; }
        [Column(128)]
        public string Competencias { get; set; }
        [Column(129)]
        public string MiniBio { get; set; }

        [Column(130)]
        public string Empresa1 { get; set; }
        [Column(131)]
        public string CargoEmpresa1 { get; set; }
        [Column(132)]
        public string DescricaoEmpresa1 { get; set; }
        [Column(133)]
        public string DataInicioEmpresa1 { get; set; }
        [Column(134)]
        public string DataSaidaEmpresa1 { get; set; }

        [Column(135)]
        public string Empresa2 { get; set; }
        [Column(136)]
        public string CargoEmpresa2 { get; set; }
        [Column(137)]
        public string DescricaoEmpresa2 { get; set; }
        [Column(138)]
        public string DataInicioEmpresa2 { get; set; }
        [Column(139)]
        public string DataSaidaEmpresa2 { get; set; }

        [Column(140)]
        public string EmpresaN { get; set; }
        [Column(141)]
        public string CargoEmpresaN { get; set; }
        [Column(142)]
        public string DescricaoEmpresaN { get; set; }
        [Column(143)]
        public string DataInicioEmpresaN { get; set; }
        [Column(144)]
        public string DataSaidaEmpresaN { get; set; }

        [Column(145)]
        public string ObjetivoProfissionalCurto { get; set; }
        [Column(146)]
        public string ObjetivoProfissionalLongo { get; set; }

        [Column(147)]
        public string Programa { get; set; }
        [Column(148)]
        public string Turma { get; set; }
        [Column(149)]
        public string Presenca { get; set; }
        [Column(150)]
        public string MediaTurma { get; set; }
        [Column(151)]
        public string RankingTurma { get; set; }
        [Column(152)]
        public string PerfilProseek { get; set; }
        [Column(153)]
        public string Vies1 { get; set; }
        [Column(154)]
        public string Vies2 { get; set; }
        [Column(155)]
        public string TempoDisponivel { get; set; }
        [Column(156)]
        public string Dispositivo { get; set; }
        [Column(157)]
        public string MediaConteudo { get; set; }
        [Column(158)]
        public string Interacao { get; set; }

        [Column(159)]
        public string NomeModulo { get; set; }
        [Column(160)]
        public string IdModuloEvento { get; set; }
        [Column(161)]
        public string Pontuacao { get; set; }
        [Column(162)]
        public string UltimoBadge { get; set; }
        [Column(163)]
        public string UltimoBadgeMediaTurma { get; set; }
        [Column(164)]
        public string Pontuacao_2 { get; set; }
        [Column(165)]
        public string UltimoBadge_2 { get; set; }

        [Column(166)]
        public string CC { get; set; }
        [Column(167)]
        public string CS { get; set; }
        [Column(168)]
        public string CCS { get; set; }
        [Column(169)]
        public string REIPODO { get; set; }
        [Column(170)]
        public string ADODO { get; set; }
        [Column(171)]
        public string EAEDO { get; set; }
        [Column(172)]
        public string PA { get; set; }
        [Column(173)]
        public string FA { get; set; }
        [Column(174)]
        public string EA { get; set; }
        [Column(175)]
        public string IndicadorAtitude1 { get; set; }
        [Column(176)]
        public string IndicadorAtitude2 { get; set; }
        [Column(177)]
        public string CapacidadeTecnica { get; set; }
        [Column(178)]
        public string Postura { get; set; }
        [Column(179)]
        public string Argumentacao { get; set; }
        [Column(180)]
        public string Articulacao { get; set; }
        [Column(181)]
        public string Negociacao { get; set; }

        [Column(182)]
        public string Atitude1 { get; set; }
        [Column(183)]
        public string Atitude2 { get; set; }
        [Column(184)]
        public string Atitude3 { get; set; }
        [Column(185)]
        public string Atitude4 { get; set; }
        [Column(186)]
        public string Atitude5 { get; set; }
        [Column(187)]
        public string Atitude6 { get; set; }
        [Column(188)]
        public string Atitude7 { get; set; }
        [Column(189)]
        public string Atitude8 { get; set; }
        [Column(190)]
        public string Atitude9 { get; set; }
        [Column(191)]
        public string IndicadorAtitude1_2 { get; set; }
        [Column(192)]
        public string IndicadorAtitude2_2 { get; set; }
        [Column(193)]
        public string Habilidade1 { get; set; }
        [Column(194)]
        public string Habilidade2 { get; set; }
        [Column(195)]
        public string Habilidade3 { get; set; }
        [Column(196)]
        public string Habilidade4 { get; set; }
        [Column(197)]
        public string Habilidade5 { get; set; }

        [Column(198)]
        public string Atitude1_2 { get; set; }
        [Column(199)]
        public string Atitude2_2 { get; set; }
        [Column(200)]
        public string Atitude3_2 { get; set; }
        [Column(201)]
        public string Atitude4_2 { get; set; }
        [Column(202)]
        public string Atitude5_2 { get; set; }
        [Column(203)]
        public string Atitude6_2 { get; set; }
        [Column(204)]
        public string Atitude7_2 { get; set; }
        [Column(205)]
        public string Atitude8_2 { get; set; }
        [Column(206)]
        public string Atitude9_2 { get; set; }
        [Column(207)]
        public string IndicadorAtitude1_3 { get; set; }
        [Column(208)]
        public string IndicadorAtitude2_3 { get; set; }
        [Column(209)]
        public string Habilidade1_2 { get; set; }
        [Column(210)]
        public string Habilidade2_2 { get; set; }
        [Column(211)]
        public string Habilidade3_2 { get; set; }
        [Column(212)]
        public string Habilidade4_2 { get; set; }
        [Column(213)]
        public string Habilidade5_2 { get; set; }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class Column : Attribute
    {
        public int ColumnIndex { get; set; }

        public Column(int column)
        {
            ColumnIndex = column;
        }
    }
}
