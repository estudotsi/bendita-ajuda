import { Category } from '../../core/models';

export const MOCK_CATEGORIES: Category[] = [
  {
    id: 'eletricista',
    name: 'Eletricista',
    pluralName: 'Eletricistas',
    keywords: [
      'eletrica', 'eletrico', 'luz', 'tomada', 'chuveiro', 'disjuntor', 'fio', 'fiacao',
      'lampada', 'interruptor', 'curto', 'energia', 'queimou', 'choque', 'quadro de luz',
    ],
  },
  {
    id: 'encanador',
    name: 'Encanador',
    pluralName: 'Encanadores',
    keywords: [
      'agua', 'pia', 'vazamento', 'vazando', 'vaza', 'cano', 'torneira', 'descarga',
      'privada', 'vaso sanitario', 'entupido', 'entupida', 'entupiu', 'esgoto',
      'caixa d agua', 'registro', 'ralo', 'hidraulica',
    ],
  },
  {
    id: 'faxina',
    name: 'Faxina',
    pluralName: 'Profissionais de faxina',
    keywords: [
      'limpeza', 'limpar', 'faxineira', 'faxineiro', 'diarista', 'passar roupa', 'lavar', 'sujeira',
    ],
  },
  {
    id: 'pedreiro',
    name: 'Pedreiro',
    pluralName: 'Pedreiros',
    keywords: [
      'obra', 'reforma', 'parede', 'piso', 'reboco', 'muro', 'telhado', 'goteira', 'rachadura',
      'azulejo', 'cimento', 'construcao', 'calcada', 'laje',
    ],
  },
  {
    id: 'pintor',
    name: 'Pintor',
    pluralName: 'Pintores',
    keywords: ['pintura', 'pintar', 'tinta', 'descascando', 'mofo', 'textura', 'grafiato', 'verniz'],
  },
  {
    id: 'jardineiro',
    name: 'Jardineiro',
    pluralName: 'Jardineiros',
    keywords: ['jardim', 'grama', 'planta', 'plantas', 'poda', 'podar', 'arvore', 'mato', 'quintal', 'rocar', 'horta'],
  },
  {
    id: 'montador-de-moveis',
    name: 'Montador de móveis',
    pluralName: 'Montadores de móveis',
    keywords: [
      'montar', 'montagem', 'movel', 'moveis', 'guarda roupa', 'armario', 'cama', 'estante',
      'desmontar', 'prateleira', 'rack', 'mudanca',
    ],
  },
  {
    id: 'tecnico-ar-condicionado',
    name: 'Técnico de ar-condicionado',
    pluralName: 'Técnicos de ar-condicionado',
    keywords: ['ar', 'ar condicionado', 'split', 'climatizacao', 'nao gela', 'gelando', 'refrigeracao'],
  },
];
