
                    b.Property<string>("Descripcion")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<bool>("Estado")
                        .HasColumnType("bit");

                    b.Property<int>("ID_Categoria")
                        .HasColumnType("int");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<decimal>("Precio")
                        .HasColumnType("decimal(10,2)");

                    b.HasKey("ID_Producto");

                    b.HasIndex("ID_Categoria");

                    b.ToTable("Productos");
                });

            modelBuilder.Entity("Restaurante.Models.DetallePedido", b =>
                {
                    b.HasOne("Restaurante.Models.Pedido", "Pedido")
                        .WithMany("Detalles")
                        .HasForeignKey("ID_Pedido")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Restaurante.Models.Producto", "Producto")
                        .WithMany()
                        .HasForeignKey("ID_Producto")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Pedido");

                    b.Navigation("Producto");
                });

            modelBuilder.Entity("Restaurante.Models.Pedido", b =>
                {
                    b.HasOne("Restaurante.Models.Empleado", "Empleado")
                        .WithMany("Pedidos")
                        .HasForeignKey("ID_Empleado")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Restaurante.Models.Cliente", "Cliente")
                        .WithMany("Pedidos")
                        .HasForeignKey("ID_Cliente")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Restaurante.Models.Mesa", "Mesa")
                        .WithMany("Pedidos")
                        .HasForeignKey("ID_Mesa")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Empleado");

                    b.Navigation("Cliente");

                    b.Navigation("Mesa");
                });

            modelBuilder.Entity("Restaurante.Models.Producto", b =>
                {
                    b.HasOne("Restaurante.Models.Categoria", "Categoria")
                        .WithMany()
                        .HasForeignKey("ID_Categoria")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Categoria");
                });

            modelBuilder.Entity("Restaurante.Models.Empleado", b =>
                {
                    b.Navigation("Pedidos");
                });

            modelBuilder.Entity("Restaurante.Models.Cliente", b =>
                {
                    b.Navigation("Pedidos");
                });

            modelBuilder.Entity("Restaurante.Models.Mesa", b =>
                {
                    b.Navigation("Pedidos");
                });

            modelBuilder.Entity("Restaurante.Models.Pedido", b =>
                {
                    b.Navigation("Detalles");
                });
#pragma warning restore 612, 618
        }
    }
}