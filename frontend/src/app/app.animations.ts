import { trigger, transition, group, query, style, animate, animateChild } from '@angular/animations';

export const animations = [
  trigger('animRoutes', [
    transition('* <=> *', [
      group([
        query(':enter',
          [
            style({ opacity: 0 }),
            animate('0.35s ease-in-out', style({
              transform: 'translateX(0%)',
              opacity: 1
            }))
          ],
          { optional: true }
        ),
        query(':leave',
          [
            style({transform: 'translateX(0%)', position: 'absolute', left: 0, right: 0, top: 0}),
            animate('0.35s ease-in-out', style({ opacity: 0 }))
          ],
          { optional: true }
        )
      ])
    ])
  ])
];
