using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SmartWarehouse.Database;
using SmartWarehouse.Models;
using SmartWarehouse.Models.DTOs;
using SmartWarehouse.Models.Wrappers;
using SmartWarehouse.Services.Interfaces;

namespace SmartWarehouse.Services {
    public class PackageService : IPackageService {
        private readonly SmartWarehouseContext _context;
        public PackageService(SmartWarehouseContext context) {
            _context = context;
        }

        public async Task<Envelope<Package>> GetPackageByIdAsync(int id) {
            var package = await _context.Packages.FindAsync(id);
            if (package == null) return Envelope<Package>.Error("No such item");
            return package;
        }

        public async Task<Envelope<Package>> CreatePackageAsync(PackageCreationDTO creationDTO) {
            var package = new Package {
                TrackingNumber = creationDTO.TrackingNumber,
                Size = creationDTO.Size,
                WeightKg = creationDTO.WeightKg
            };

            try {
                await _context.AddAsync(package);
                await _context.SaveChangesAsync();

                return package;
            } catch (Exception ex) {
                return Envelope<Package>.Error($"Unable to create a package. Details: {ex}");
            }
        }
    }
}
