import { TestBed, inject } from '@angular/core/testing';

import { GetImagesService } from './get-images.service';

describe('GetImagesService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GetImagesService]
    });
  });

  it('should be created', inject([GetImagesService], (service: GetImagesService) => {
    expect(service).toBeTruthy();
  }));
});
