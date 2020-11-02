import { TestBed } from '@angular/core/testing';

import { TermGuard } from './term.guard';

describe('TermGuard', () => {
  let guard: TermGuard;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    guard = TestBed.inject(TermGuard);
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });
});
