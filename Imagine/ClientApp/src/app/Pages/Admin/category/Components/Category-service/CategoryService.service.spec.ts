/* tslint:disable:no-unused-variable */

import { TestBed,  inject } from '@angular/core/testing';
import { CategoryService } from './CategoryService.service';

describe('Service: CategoryService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CategoryService]
    });
  });

  it('should ...', inject([CategoryService], (service: CategoryService) => {
    expect(service).toBeTruthy();
  }));
});
