export interface UserCategoryFilter {
  name?: string;
  title?: string;
  category: number;
  filterColumn: string;
  page: number;
  itemsCount: number;
  maxLength?: number;
  options: Array<CategoryFilterOption>;
  selectedOptions: Array<CategoryFilterOption>;

  isAlternate?: boolean;
  demandsId?: boolean;
  hideSearch?: boolean;
  haveCustomInput?: boolean;
}

export interface CategoryFilterOption {
  id: string;
  description: string;
  checked: boolean;
  customInput?: boolean;
}

export interface UserCategoryFilterSearchOption {
  columnName: string;
  contentNames: string[];
}

export interface UserCategoryFilterSearch {
  value: string;
  filter: UserCategoryFilter;
  page: number;
  prop: string;
}
