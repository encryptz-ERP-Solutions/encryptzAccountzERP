import { MAT_DATE_FORMATS, MatDateFormats } from '@angular/material/core';

export const MY_DATE_FORMATS: MatDateFormats = {
  parse: {
    dateInput: 'DD-MM-YYYY', // Custom date format for parsing input
  },
  display: {
    dateInput: 'DD-MM-YYYY', // Custom date format for displaying date in input
    monthYearLabel: 'MMM YYYY',
    monthYearA11yLabel: 'MMMM YYYY',
    dateA11yLabel: 'YYYY',
  },
};
