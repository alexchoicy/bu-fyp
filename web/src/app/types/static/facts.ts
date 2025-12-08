export enum AvailableTerm {
  Semester1 = 1,
  Semester2 = 2,
  SummerTerm = 3,
}

export const AvailableTermLabels: Record<AvailableTerm, string> = {
  [AvailableTerm.Semester1]: 'Semester 1',
  [AvailableTerm.Semester2]: 'Semester 2',
  [AvailableTerm.SummerTerm]: 'Summer Term',
}
